using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    /// <summary>
    ///     Represents the file selection control.
    /// </summary>
    public partial class PathSelectionControl
    {
        #region Constants and Fields

        public static readonly DependencyProperty ModeProperty =
            Helper.RegisterDependencyProperty(
                (PathSelectionControl obj) => obj.Mode,
                new FrameworkPropertyMetadata(PathSelectionControlMode.FileSelection),
                value => value is PathSelectionControlMode && ((PathSelectionControlMode)value).IsDefined());

        public static readonly DependencyProperty SelectedPathProperty =
            Helper.RegisterDependencyProperty(
                (PathSelectionControl obj) => obj.SelectedPath,
                new FrameworkPropertyMetadata(string.Empty, OnSelectedPathChanged));

        public static readonly DependencyProperty FileDialogFilterProperty =
            Helper.RegisterDependencyProperty((PathSelectionControl obj) => obj.FileDialogFilter);

        public static readonly DependencyProperty DialogTitleProperty =
            Helper.RegisterDependencyProperty((PathSelectionControl obj) => obj.DialogTitle);

        public static readonly DependencyProperty IsClearButtonVisibleProperty =
            Helper.RegisterDependencyProperty(
                (PathSelectionControl obj) => obj.IsClearButtonVisible,
                new FrameworkPropertyMetadata(DefaultIsClearButtonVisible, OnIsClearButtonVisibleChanged));

        private const bool DefaultIsClearButtonVisible = false;

        #endregion

        #region Constructors

        public PathSelectionControl()
        {
            InitializeComponent();

            UpdateSelectedPath();
            UpdateClearButtonVisibility();
            UpdateClearButtonEnabledState();
        }

        #endregion

        #region Public Properties

        public PathSelectionControlMode Mode
        {
            [DebuggerNonUserCode]
            get
            {
                return (PathSelectionControlMode)GetValue(ModeProperty);
            }

            [DebuggerNonUserCode]
            set
            {
                SetValue(ModeProperty, value);
            }
        }

        public string SelectedPath
        {
            [DebuggerNonUserCode]
            get
            {
                return (string)GetValue(SelectedPathProperty);
            }

            [DebuggerNonUserCode]
            set
            {
                SetValue(SelectedPathProperty, value);
            }
        }

        public string FileDialogFilter
        {
            [DebuggerNonUserCode]
            get
            {
                return (string)GetValue(FileDialogFilterProperty);
            }

            [DebuggerNonUserCode]
            set
            {
                SetValue(FileDialogFilterProperty, value);
            }
        }

        public string DialogTitle
        {
            [DebuggerNonUserCode]
            get
            {
                return (string)GetValue(DialogTitleProperty);
            }

            [DebuggerNonUserCode]
            set
            {
                SetValue(DialogTitleProperty, value);
            }
        }

        public bool IsClearButtonVisible
        {
            [DebuggerNonUserCode]
            get
            {
                return (bool)GetValue(IsClearButtonVisibleProperty);
            }

            [DebuggerNonUserCode]
            set
            {
                SetValue(IsClearButtonVisibleProperty, value);
            }
        }

        #endregion

        #region Private Methods

        private static void OnSelectedPathChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            var self = (PathSelectionControl)dependencyObject.EnsureNotNull();
            self.FilePathTextBox.Text = self.SelectedPath;
            self.UpdateClearButtonEnabledState();
        }

        private static void OnIsClearButtonVisibleChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var self = (PathSelectionControl)dependencyObject.EnsureNotNull();
            self.UpdateClearButtonVisibility();
        }

        private void UpdateSelectedPath()
        {
            this.SelectedPath = this.FilePathTextBox.Text;
        }

        private void UpdateClearButtonVisibility()
        {
            this.ClearButton.Visibility = this.IsClearButtonVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateClearButtonEnabledState()
        {
            this.ClearButton.IsEnabled = !this.SelectedPath.IsNullOrEmpty();
        }

        private void SelectFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                FileName = this.SelectedPath,
                Filter = this.FileDialogFilter,
                Multiselect = false,
                ShowReadOnly = false,
                Title = this.DialogTitle
            };

            if (!openFileDialog.ShowDialog(this.GetControlWindow()).GetValueOrDefault())
            {
                return;
            }

            this.SelectedPath = openFileDialog.FileName;
        }

        private void SelectDirectory()
        {
            using (
                var folderBrowserDialog = new FolderBrowserDialog
                {
                    Description = this.DialogTitle,
                    SelectedPath = this.SelectedPath,
                    ShowNewFolderButton = true
                })
            {
                var dialogResult = folderBrowserDialog.ShowDialog(this.GetControlWindow().GetWin32Window());
                if (dialogResult != DialogResult.OK)
                {
                    return;
                }

                this.SelectedPath = folderBrowserDialog.SelectedPath;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            switch (this.Mode)
            {
                case PathSelectionControlMode.FileSelection:
                    SelectFile();
                    break;

                case PathSelectionControlMode.DirectorySelection:
                    SelectDirectory();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSelectedPath();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedPath = string.Empty;
        }

        #endregion
    }
}