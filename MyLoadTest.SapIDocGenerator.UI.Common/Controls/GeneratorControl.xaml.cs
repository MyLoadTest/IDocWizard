using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using HP.LR.VuGen.ServiceCore.Data.ProjectSystem;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Win32;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public partial class GeneratorControl
    {
        #region Constants and Fields

        private const string MainActionName = "Action";

        #endregion

        #region Constructors

        public GeneratorControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var currentProject = ProjectService.CurrentProject as IVuGenProject;
            if (currentProject == null)
            {
                this.ShowErrorBox("No VuGen project is loaded.");
                return;
            }

            if (currentProject.ReadOnly)
            {
                this.ShowErrorBox("The current VuGen project is read-only.");
                return;
            }

            var script = currentProject.Script;
            if (script == null)
            {
                this.ShowErrorBox("A VuGen script is not assigned in the current project.");
                return;
            }

            var action = script.GetActionByName(MainActionName);
            if (action == null)
            {
                this.ShowErrorBox(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "VuGen script action '{0}' is not found.",
                        MainActionName));
                return;
            }

            var confirmMessage = string.Format(
                CultureInfo.InvariantCulture,
                "The file '{0}' will be overwritten with the generated contents.{1}{1}Do you want to continue?",
                action.FullFileName,
                Environment.NewLine);
            var messageBoxResult = this.ShowMessageBox(
                confirmMessage,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var definition = SapIDocDefinition.LoadHeader(this.ViewModel.WizardPage.DefinitionFilePath);
                var idocText = File.ReadAllText(this.ViewModel.WizardPage.ExampleFilePath);
                var doc = new SapIDoc(definition, idocText);
                var actionContents = doc.GetVuGenActionContents();

                File.WriteAllText(action.FullFileName, actionContents, Encoding.Default);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                this.ShowErrorBox(ex, "Error creating script");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Reset();
        }

        private void NewTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ViewModel.ImportPage.IsRepositoryPathSelected)
            {
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = Properties.Resources.NewTypePathSelectionControlFilter,
                Multiselect = false,
                ShowReadOnly = false
            };

            if (!openFileDialog.ShowDialog(this.GetControlWindow()).GetValueOrDefault())
            {
                return;
            }

            try
            {
                var filePath = openFileDialog.FileName;
                var definition = SapIDocDefinition.LoadHeader(filePath);

                var path = Path.Combine(this.ViewModel.ImportPage.RepositoryPath, definition.Name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var destinationFilePath = Path.Combine(path, Path.GetFileName(filePath));
                File.Copy(filePath, destinationFilePath, true);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                this.ShowErrorBox(ex, "Error importing a new type");
            }

            this.ViewModel.ImportPage.RefreshRepositoryItems();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ViewModel.ImportPage.IsRepositoryPathSelected)
            {
                return;
            }

            var collectionView = this.RepositoryItemsListView.ItemsSource as ICollectionView;
            if (collectionView == null || collectionView.CurrentItem == null)
            {
                return;
            }

            var repositoryItem = collectionView.CurrentItem as RepositoryItem;
            if (repositoryItem == null)
            {
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = Properties.Resources.ImportPathSelectionControlFilter,
                Multiselect = true,
                ShowReadOnly = false
            };

            if (!openFileDialog.ShowDialog(this.GetControlWindow()).GetValueOrDefault()
                || !openFileDialog.FileNames.Any())
            {
                return;
            }

            var filePaths = openFileDialog.FileNames;

            try
            {
                var definition = SapIDocDefinition.LoadHeader(repositoryItem.DefinitionFilePath);
                foreach (var filePath in filePaths)
                {
                    var contents = File.ReadAllText(filePath);
                    var doc = new SapIDoc(definition, contents);
                    var resultingFileContents = doc.GetXml().ToString(SaveOptions.None);

                    var resultingFilePath = Path.Combine(
                        this.ViewModel.ImportPage.RepositoryPath,
                        repositoryItem.Folder,
                        string.Format(CultureInfo.InvariantCulture, "{0}.txt", doc.Number));

                    File.WriteAllText(resultingFilePath, resultingFileContents);
                }
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                this.ShowErrorBox(ex, "Error importing IDoc file(s)");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ImportPage.RefreshRepositoryItems();
        }

        #endregion
    }
}