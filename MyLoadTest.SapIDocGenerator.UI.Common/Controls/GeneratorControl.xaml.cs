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

        private const string MainActionName = "Action";

        #endregion

        #region Constructors

        public GeneratorControl()
        {
            InitializeComponent();

            this.ParametersPage.ViewModel = this.ViewModel;  // Synchronizing model instance
        }

        #endregion

        #region Public Methods

        public void ActivateTab(GeneratorControlTab tab)
        {
            TabItem tabItem;
            switch (tab)
            {
                case GeneratorControlTab.Wizard:
                    tabItem = this.WizardTab;
                    break;

                case GeneratorControlTab.Import:
                    tabItem = this.ImportTab;
                    break;

                case GeneratorControlTab.Parameters:
                    tabItem = this.ParametersTab;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("tab", tab, null);
            }

            this.Tabs.SelectedItem = tabItem;
        }

        public void ActivateParameters()
        {
            ActivateTab(GeneratorControlTab.Parameters);
            this.ParametersPage.ActivateControl();
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

            var actionItem = script.GetActionByName(MainActionName);
            if (actionItem == null)
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
                "The file '{0}' WILL be overwritten with the generated contents.{2}{2}"
                    + "The file '{1}' MAY also be updated.{2}{2}{2}"
                    + "Do you want to continue?",
                actionItem.FullFileName,
                script.FileName,
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
                this.ViewModel.WizardPage.GenerateAction(script, actionItem);
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