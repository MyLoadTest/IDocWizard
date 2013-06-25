using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HP.LR.VuGen.ServiceCore.Data.ProjectSystem;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Win32;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public partial class GeneratorControl
    {
        #region Constants and Fields

        private const string MainActionName = "Action";
        private const string IdocParameterFormat = "{{IDoc:{0}:{1}}}";

        #endregion

        #region Constructors

        public GeneratorControl()
        {
            InitializeComponent();
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

            var controlToFocus = this.ViewModel.ParametersPage.IdocItemsView.CurrentItem == null
                ? (Control)this.IdocItemsComboBox
                : this.ParameterTreeView;

            if (controlToFocus == null)
            {
                return;
            }

            var isVisibleChanged = new ValueHolder<DependencyPropertyChangedEventHandler>();

            isVisibleChanged.Value =
                (sender, args) =>
                {
                    if (!controlToFocus.IsVisible)
                    {
                        return;
                    }

                    var f = controlToFocus.Focus();
                    System.Diagnostics.Trace.WriteLine(f);

                    controlToFocus.IsVisibleChanged -= isVisibleChanged.Value.EnsureNotNull();
                };

            if (controlToFocus.IsVisible)
            {
                isVisibleChanged.Value(controlToFocus, new DependencyPropertyChangedEventArgs());
            }
            else
            {
                controlToFocus.IsVisibleChanged += isVisibleChanged.Value.EnsureNotNull();
            }
        }

        #endregion

        #region Private Methods

        private static bool TryReplaceWithParameter(
            string parameter,
            out UIElement popupElement,
            out Point? popupPoint)
        {
            popupElement = WorkbenchSingleton.MainWindow.EnsureNotNull();
            popupPoint = null;

            var workbench = WorkbenchSingleton.Workbench.EnsureNotNull();
            object activeViewContent = workbench.ActiveViewContent;
            var textEditorProvider = activeViewContent as ITextEditorProvider;
            if (textEditorProvider == null)
            {
                return false;
            }

            var textEditor = textEditorProvider.TextEditor;
            if (textEditor == null)
            {
                return false;
            }

            if (textEditor.SelectedText.IsNullOrEmpty())
            {
                return false;
            }

            var viewContent = activeViewContent as IViewContent;
            if (viewContent != null)
            {
                var textArea = viewContent.InitiallyFocusedControl as TextArea;
                if (textArea != null && textArea.TextView != null)
                {
                    popupElement = textArea.TextView;

                    var visualPosition = textArea.TextView.GetVisualPosition(
                        new TextViewPosition(textArea.Caret.Line, textArea.Caret.Column),
                        VisualYPosition.LineBottom);
                    popupPoint = visualPosition - textArea.TextView.ScrollOffset;
                }
            }

            textEditor.SelectedText = parameter;
            return true;
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

        private void ReplaceOrCopyButton_Click(object sender, RoutedEventArgs e)
        {
            var fieldTreeNode = this.ViewModel.ParametersPage.GetSelectedIdocTreeNode();
            if (fieldTreeNode == null || fieldTreeNode.Parent == null)
            {
                return;
            }

            var segmentTreeNode = fieldTreeNode.Parent.EnsureNotNull();

            var parameter = string.Format(
                CultureInfo.InvariantCulture,
                IdocParameterFormat,
                segmentTreeNode.Name,
                fieldTreeNode.Name);

            UIElement popupElement;
            Point? popupPoint;
            if (TryReplaceWithParameter(parameter, out popupElement, out popupPoint))
            {
                UIHelper.ShowInfoPopup(popupElement, Properties.Resources.ReplacedWithParameterPopupText, popupPoint);
                return;
            }

            Clipboard.SetText(parameter);
            UIHelper.ShowInfoPopup(this, Properties.Resources.CopiedToClipboardPopupText);
        }

        #endregion
    }
}