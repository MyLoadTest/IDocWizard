using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    /// <summary>
    ///     Represents the file selection control.
    /// </summary>
    public partial class FileSelectionControl
    {
        #region Constants and Fields

        public static readonly DependencyProperty FilePathProperty =
            Helper.RegisterDependencyProperty(
                (FileSelectionControl obj) => obj.FilePath,
                new FrameworkPropertyMetadata(string.Empty, OnFilePathChanged));

        public static readonly DependencyProperty FileDialogFilterProperty =
            Helper.RegisterDependencyProperty((FileSelectionControl obj) => obj.FileDialogFilter);

        public static readonly DependencyProperty FileDialogTitleProperty =
            Helper.RegisterDependencyProperty((FileSelectionControl obj) => obj.FileDialogTitle);

        #endregion

        #region Constructors

        public FileSelectionControl()
        {
            InitializeComponent();

            UpdateFilePath();
        }

        #endregion

        #region Public Properties

        public string FilePath
        {
            [DebuggerNonUserCode]
            get
            {
                return (string)GetValue(FilePathProperty);
            }

            [DebuggerNonUserCode]
            set
            {
                SetValue(FilePathProperty, value);
            }
        }

        public string FileDialogFilter
        {
            get
            {
                return (string)GetValue(FileDialogFilterProperty);
            }

            set
            {
                SetValue(FileDialogFilterProperty, value);
            }
        }

        public string FileDialogTitle
        {
            get
            {
                return (string)GetValue(FileDialogTitleProperty);
            }

            set
            {
                SetValue(FileDialogTitleProperty, value);
            }
        }

        #endregion

        #region Private Methods

        private static void OnFilePathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var self = (FileSelectionControl)dependencyObject;
            self.FilePathTextBox.SetValue(TextBox.TextProperty, self.FilePath);
        }

        private void UpdateFilePath()
        {
            SetValue(FilePathProperty, this.FilePathTextBox.GetValue(TextBox.TextProperty));
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = this.FileDialogFilter,
                Multiselect = false,
                ShowReadOnly = false,
                Title = this.FileDialogTitle
            };

            if (!openFileDialog.ShowDialog(this.GetControlWindow()).GetValueOrDefault())
            {
                return;
            }

            this.FilePath = openFileDialog.FileName;
        }

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilePath();
        }

        #endregion
    }
}