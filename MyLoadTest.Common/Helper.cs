﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Application = System.Windows.Application;
using Control = System.Windows.Controls.Control;
using IWin32Window = System.Windows.Forms.IWin32Window;
using MessageBox = System.Windows.MessageBox;

namespace MyLoadTest
{
    public static class Helper
    {
        #region Constants

        private const bool DefaultInheritAttributeParameter = true;

        /// <summary>
        ///     The invalid expression message format.
        /// </summary>
        private const string InvalidExpressionMessageFormat =
            "Invalid expression (must be a getter of a property of the type '{0}'): {{ {1} }}.";

        /// <summary>
        ///     The invalid expression message auto format.
        /// </summary>
        private const string InvalidExpressionMessageAutoFormat =
            "Invalid expression (must be a getter of a property of some type): {{ {0} }}.";

        #endregion

        #region Public Methods

        [DebuggerNonUserCode]
        public static T EnsureNotNull<T>(this T value)
            where T : class
        {
            #region Argument Check

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            #endregion

            return value;
        }

        public static void DoForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            #region Argument Check

            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsFatal(this Exception exception)
        {
            return exception is ThreadAbortException
                || exception is StackOverflowException
                || exception is OutOfMemoryException;
        }

        public static string GetLocalPath(this Assembly assembly)
        {
            #region Argument Check

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            #endregion

            var uri = new Uri(assembly.CodeBase, UriKind.Absolute);
            if (!uri.IsFile)
            {
                throw new InvalidOperationException("The assembly does not have the local path.");
            }

            return uri.LocalPath.EnsureNotNull();
        }

        public static string GetDirectory(this Assembly assembly)
        {
            var path = GetLocalPath(assembly);
            return Path.GetDirectoryName(path).EnsureNotNull();
        }

        public static PropertyInfo GetPropertyInfo<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression)
        {
            #region Argument Check

            if (propertyGetterExpression == null)
            {
                throw new ArgumentNullException("propertyGetterExpression");
            }

            #endregion

            var objectType = typeof(TObject);

            var memberExpression = propertyGetterExpression.Body as MemberExpression;
            if ((memberExpression == null) || (memberExpression.NodeType != ExpressionType.MemberAccess))
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageFormat, objectType.FullName, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            var result = memberExpression.Member as PropertyInfo;
            if (result == null)
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageFormat, objectType.FullName, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            if ((result.DeclaringType == null) || !result.DeclaringType.IsAssignableFrom(objectType))
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageFormat, objectType.FullName, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            if (memberExpression.Expression == null)
            {
                var accessor = result.GetGetMethod(true) ?? result.GetSetMethod(true);
                if ((accessor == null) || !accessor.IsStatic || (result.ReflectedType != objectType))
                {
                    throw new ArgumentException(
                        string.Format(InvalidExpressionMessageFormat, objectType.FullName, propertyGetterExpression),
                        "propertyGetterExpression");
                }
            }
            else
            {
                var parameterExpression = memberExpression.Expression as ParameterExpression;
                if ((parameterExpression == null) || (parameterExpression.NodeType != ExpressionType.Parameter) ||
                    (parameterExpression.Type != typeof(TObject)))
                {
                    throw new ArgumentException(
                        string.Format(InvalidExpressionMessageFormat, objectType.FullName, propertyGetterExpression),
                        "propertyGetterExpression");
                }
            }

            return result;
        }

        public static string GetPropertyName<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression)
        {
            var propertyInfo = GetPropertyInfo(propertyGetterExpression);
            return propertyInfo.Name;
        }

        public static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TProperty>> propertyGetterExpression)
        {
            #region Argument Check

            if (propertyGetterExpression == null)
            {
                throw new ArgumentNullException("propertyGetterExpression");
            }

            #endregion

            var memberExpression = propertyGetterExpression.Body as MemberExpression;
            if ((memberExpression == null) || (memberExpression.NodeType != ExpressionType.MemberAccess))
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageAutoFormat, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            var result = memberExpression.Member as PropertyInfo;
            if (result == null)
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageAutoFormat, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            if (result.DeclaringType == null)
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageAutoFormat, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            if (memberExpression.Expression == null)
            {
                var accessor = result.GetGetMethod(true) ?? result.GetSetMethod(true);
                if ((accessor == null) || !accessor.IsStatic)
                {
                    throw new ArgumentException(
                        string.Format(InvalidExpressionMessageAutoFormat, propertyGetterExpression),
                        "propertyGetterExpression");
                }
            }

            return result;
        }

        public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> propertyGetterExpression)
        {
            var propertyInfo = GetPropertyInfo(propertyGetterExpression);
            return propertyInfo.Name;
        }

        public static string GetQualifiedName(this MethodBase method)
        {
            #region Argument Check

            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            #endregion

            var type = (method.DeclaringType ?? method.ReflectedType).EnsureNotNull();
            return type.Name + Type.Delimiter + method.Name;
        }

        public static TAttribute GetSoleAttribute<TAttribute>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit)
        {
            #region Argument Check

            if (attributeProvider == null)
            {
                throw new ArgumentNullException("attributeProvider");
            }

            #endregion

            return GetSoleAttributeInternal<TAttribute>(attributeProvider, inherit, Enumerable.SingleOrDefault);
        }

        public static TAttribute GetSoleAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider)
        {
            return GetSoleAttribute<TAttribute>(attributeProvider, DefaultInheritAttributeParameter);
        }

        public static TAttribute GetSoleAttributeStrict<TAttribute>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit)
        {
            #region Argument Check

            if (attributeProvider == null)
            {
                throw new ArgumentNullException("attributeProvider");
            }

            #endregion

            return GetSoleAttributeInternal<TAttribute>(attributeProvider, inherit, Enumerable.Single);
        }

        public static TAttribute GetSoleAttributeStrict<TAttribute>(this ICustomAttributeProvider attributeProvider)
        {
            return GetSoleAttributeStrict<TAttribute>(attributeProvider, DefaultInheritAttributeParameter);
        }

        public static bool IsDefined(this Enum value)
        {
            #region Argument Check

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            #endregion

            return Enum.IsDefined(value.GetType(), value);
        }

        public static Window GetControlWindow(this Control control)
        {
            Window result = null;

            var currentControl = (FrameworkElement)control;
            while (currentControl != null && (result = currentControl as Window) == null)
            {
                currentControl = currentControl.Parent as FrameworkElement;
            }

            return result ?? Application.Current.MainWindow;
        }

        public static IWin32Window GetWin32Window(this Window window)
        {
            if (window == null)
            {
                return WpfWin32Window.None;
            }

            var hwndSource = PresentationSource.FromVisual(window) as HwndSource;
            if (hwndSource == null || hwndSource.IsDisposed)
            {
                return WpfWin32Window.None;
            }

            return new WpfWin32Window(hwndSource);
        }

        public static MessageBoxResult ShowMessageBox(
            this Window window,
            string message,
            MessageBoxButton button,
            MessageBoxImage icon)
        {
            var actualWindow = window ?? Application.Current.MainWindow;
            return MessageBox.Show(actualWindow, message, actualWindow.Title, button, icon);
        }

        public static MessageBoxResult ShowMessageBox(
            this Control control,
            string message,
            MessageBoxButton button,
            MessageBoxImage icon)
        {
            #region Argument Check

            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            #endregion

            var window = GetControlWindow(control);
            return ShowMessageBox(window, message, button, icon);
        }

        public static MessageBoxResult ShowErrorBox(this Control control, string message)
        {
            return ShowMessageBox(control, message, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowErrorBox(this Control control, Exception exception, string baseMessage)
        {
            var message = string.Format(
                "{0}: [{1}] {2}",
                baseMessage,
                exception.GetType().Name,
                exception.Message);

            ShowErrorBox(control, message);
        }

        public static bool DefaultEquals<T>(T left, T right)
        {
            return EqualityComparer<T>.Default.Equals(left, right);
        }

        public static string ToFixedString(this DateTime value)
        {
            return value.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss", CultureInfo.InvariantCulture);
        }

        public static string ToFixedString(this DateTimeOffset value)
        {
            return value.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss' UTC'zzz", CultureInfo.InvariantCulture);
        }

        public static string GetDescription(this Enum enumValue)
        {
            #region Argument Check

            if (enumValue == null)
            {
                throw new ArgumentNullException("enumValue");
            }

            #endregion

            var type = enumValue.GetType();
            var stringValue = enumValue.ToString();

            var field = type.GetField(stringValue, BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                return stringValue;
            }

            var descriptionAttributes = field
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .ToArray();

            return descriptionAttributes.Length == 1 ? descriptionAttributes[0].Description : stringValue;
        }

        public static DependencyProperty RegisterDependencyProperty<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression,
            PropertyMetadata typeMetadata = null,
            ValidateValueCallback validateValueCallback = null)
        {
            var propertyInfo = GetPropertyInfo(propertyGetterExpression);

            if (propertyInfo.DeclaringType != typeof(TObject))
            {
                throw new ArgumentException(
                    @"Inconsistency between property expression and declared object type.",
                    "propertyGetterExpression");
            }

            return DependencyProperty.Register(
                propertyInfo.Name,
                propertyInfo.PropertyType,
                propertyInfo.DeclaringType.EnsureNotNull(),
                typeMetadata,
                validateValueCallback);
        }

        #endregion

        #region Private Methods

        private static TAttribute GetSoleAttributeInternal<TAttribute>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit,
            Func<IEnumerable<TAttribute>, TAttribute> getter)
        {
            #region Argument Check

            if (attributeProvider == null)
            {
                throw new ArgumentNullException("attributeProvider");
            }

            #endregion

            var attributes = attributeProvider.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>();
            return getter(attributes);
        }

        #endregion

        #region WpfWin32Window Class

        private sealed class WpfWin32Window : IWin32Window
        {
            #region Constants and Fields

            public static readonly WpfWin32Window None = new WpfWin32Window(null);

            private readonly HwndSource _hwndSource;

            #endregion

            #region Constructors

            public WpfWin32Window(HwndSource hwndSource)
            {
                _hwndSource = hwndSource;
            }

            #endregion

            #region IWin32Window Members

            public IntPtr Handle
            {
                [DebuggerNonUserCode]
                get
                {
                    return _hwndSource == null ? IntPtr.Zero : _hwndSource.Handle;
                }
            }

            #endregion
        }

        #endregion
    }
}