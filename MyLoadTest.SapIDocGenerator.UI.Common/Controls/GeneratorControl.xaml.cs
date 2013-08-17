using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HP.LR.VuGen.ServiceCore.Data.ProjectSystem;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Win32;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public partial class GeneratorControl
    {
        #region Constants and Fields

        private const string InitActionName = "vuser_init";
        private const string EndActionName = "vuser_end";

        #endregion

        #region Constructors

        public GeneratorControl()
        {
            InitializeComponent();

            this.Loaded += this.OnLoaded;

            this.ParametersPage.ViewModel = this.ViewModel;  // Synchronizing model instance
        }

        #endregion

        #region Public Methods

        public void ActivateTab(GeneratorControlTabName tabName)
        {
            TabItem tabItem;
            switch (tabName)
            {
                case GeneratorControlTabName.Wizard:
                    tabItem = this.WizardTab;
                    break;

                case GeneratorControlTabName.Import:
                    tabItem = this.ImportTab;
                    break;

                case GeneratorControlTabName.Parameters:
                    tabItem = this.ParametersTab;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("tabName", tabName, null);
            }

            this.Tabs.SelectedItem = tabItem;
        }

        public void ActivateParameters()
        {
            ActivateTab(GeneratorControlTabName.Parameters);
            this.ParametersPage.ActivateControl();
        }

        #endregion

        #region Private Methods

        private IActionScriptItem FindScriptAction(IVuGenScript script, string actionName)
        {
            var result = script.GetActionByName(actionName);
            if (result != null)
            {
                return result;
            }

            this.ShowErrorBox(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "VuGen script action '{0}' is not found.",
                    actionName));

            return null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Reset(true);
        }

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

            var mainActionItem = script.FindDefaultAction();
            if (mainActionItem == null)
            {
                return;
            }

            var initActionItem = FindScriptAction(script, InitActionName);
            if (initActionItem == null)
            {
                return;
            }

            var endActionItem = FindScriptAction(script, EndActionName);
            if (endActionItem == null)
            {
                return;
            }

            var actionFilePaths = new[] { initActionItem, mainActionItem, endActionItem }
                .Select(item => item.FullFileName)
                .Join("'," + Environment.NewLine + "'");

            var confirmMessage = string.Format(
                CultureInfo.InvariantCulture,
                "The following files WILL be overwritten with the generated contents:{0}"
                    + "'{1}'.{0}{0}"
                    + "The following file MAY also be updated (and reopened to reflect the changes):{0}"
                    + "'{2}'.{0}{0}"
                    + "Do you want to continue?",
                Environment.NewLine,
                actionFilePaths,
                script.FileName);

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
                this.ViewModel.WizardPage.GenerateAction(script, initActionItem, mainActionItem, endActionItem);
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
            this.ViewModel.WizardPage.Reset(false);
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
                this.ViewModel.ImportPage.CreateNewType(openFileDialog.FileName);
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

            try
            {
                this.ViewModel.ImportPage.ImportIdocFiles(repositoryItem, openFileDialog.FileNames);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                this.ShowErrorBox(ex, "Error importing IDoc file(s)");
            }

            this.ViewModel.ImportPage.RefreshRepositoryItems();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ImportPage.RefreshRepositoryItems();
        }

        #endregion
    }
}