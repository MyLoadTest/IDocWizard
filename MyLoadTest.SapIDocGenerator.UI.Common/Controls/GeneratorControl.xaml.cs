using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
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

            var definition = SapIDocDefinition.LoadHeader(this.ViewModel.WizardPage.DefinitionFilePath);
            var idocText = File.ReadAllText(this.ViewModel.WizardPage.ExampleFilePath);
            var doc = new SapIDoc(definition, idocText);
            var actionContents = doc.GetVuGenActionContents();

            File.WriteAllText(action.FullFileName, actionContents, Encoding.Default);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Reset();
        }

        private void NewTypeButton_Click(object sender, RoutedEventArgs e)
        {
            var repositoryPath = this.ViewModel.ImportPage.RepositoryPath;
            if (repositoryPath.IsNullOrWhiteSpace())
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
                var docDefinition = SapIDocDefinition.LoadHeader(openFileDialog.FileName);

                var path = Path.Combine(repositoryPath, docDefinition.Name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var destinationFilePath = Path.Combine(path, Path.GetFileName(openFileDialog.FileName));
                File.Copy(openFileDialog.FileName, destinationFilePath, true);

                this.ViewModel.ImportPage.RefreshRepositoryItems();
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                this.ShowErrorBox(ex, "Error importing a new type");
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            this.ShowErrorBox(new NotImplementedException().Message);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ImportPage.RefreshRepositoryItems();
        }

        #endregion
    }
}