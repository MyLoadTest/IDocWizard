using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public partial class ParametersPageControl
    {
        #region Constants and Fields

        private const string IdocParameterFormat = "{{IDoc:{0}:{1}}}";

        #endregion

        #region Constructors

        public ParametersPageControl()
        {
            InitializeComponent();

            this.Loaded += this.Control_Loaded;
            ChangeViewModelEventSubscription(true);
        }

        #endregion

        #region Events

        public event EventHandler<ParametersPageControlActionExecutedEventArgs> ActionExecuted;

        #endregion

        #region Public Properties

        public GeneratorControlViewModel ViewModel
        {
            [DebuggerNonUserCode]
            get
            {
                return this.InternalViewModel.EnsureNotNull();
            }

            set
            {
                #region Argument Check

                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                #endregion

                ChangeViewModelEventSubscription(false);
                this.InternalViewModel = value;
                ChangeViewModelEventSubscription(true);
            }
        }

        public TreeView ParameterTreeViewReference
        {
            [DebuggerNonUserCode]
            get
            {
                return this.ParameterTreeView;
            }
        }

        #endregion

        #region Public Methods

        public void ActivateControl()
        {
            var controlToFocus = this.ViewModel.ParametersPage.IdocItemsView.CurrentItem == null
                ? (Control)this.IdocItemsComboBox
                : this.ParameterTreeView;

            if (controlToFocus == null)
            {
                return;
            }

            var isVisibleChanged = new ValueHolder<DependencyPropertyChangedEventHandler>();

            isVisibleChanged.Value =
                (sender, e) =>
                {
                    if (!controlToFocus.IsVisible)
                    {
                        return;
                    }

                    var f = controlToFocus.Focus();
                    Trace.WriteLine(f);

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

        public void PerformAction()
        {
            DoAction();
        }

        #endregion

        #region Protected Methods

        protected virtual void RaiseActionExecuted(bool replaced)
        {
            var handler = this.ActionExecuted;
            if (handler != null)
            {
                handler(this, new ParametersPageControlActionExecutedEventArgs(replaced));
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

            TextArea textArea = null;

            var viewContent = activeViewContent as IViewContent;
            if (viewContent != null)
            {
                textArea = viewContent.InitiallyFocusedControl as TextArea;
                if (textArea != null && textArea.TextView != null)
                {
                    textArea.Caret.BringCaretToView();

                    popupElement = textArea.TextView;

                    var visualPosition = textArea.TextView.GetVisualPosition(
                        new TextViewPosition(textArea.Caret.Line, textArea.Caret.Column),
                        VisualYPosition.LineBottom);
                    popupPoint = visualPosition - textArea.TextView.ScrollOffset;
                }
            }

            textEditor.SelectedText = parameter;

            if (textArea != null)
            {
                textArea.Caret.BringCaretToView();
            }

            return true;
        }

        private void ChangeViewModelEventSubscription(bool subscribe)
        {
            if (this.InternalViewModel == null)
            {
                return;
            }

            if (subscribe)
            {
                this.InternalViewModel.ParametersPage.PropertyChanged += this.ParametersPage_PropertyChanged;
            }
            else
            {
                this.InternalViewModel.ParametersPage.PropertyChanged -= this.ParametersPage_PropertyChanged;
            }
        }

        private void FocusSelectedNode()
        {
            var selectedNode = this.ViewModel.ParametersPage.GetSelectedIdocTreeNode();
            if (selectedNode == null)
            {
                return;
            }

            var stack = new Stack<IdocTreeNode>();
            var currentNode = selectedNode;
            while (currentNode != null)
            {
                stack.Push(currentNode);

                currentNode = currentNode.Parent;
            }

            var itemsControl = (ItemsControl)this.ParameterTreeView;
            while (stack.Count > 0)
            {
                var node = stack.Pop();

                var tvi = (TreeViewItem)itemsControl.ItemContainerGenerator.ContainerFromItem(node);
                if (tvi == null)
                {
                    break;
                }

                if (stack.Count == 0)
                {
                    FocusManager.SetFocusedElement(this, this.ParameterTreeView);
                    tvi.Focus();
                    break;
                }

                tvi.IsExpanded = true;
                tvi.UpdateLayout();  // Forcing to generate child items
                itemsControl = tvi;
            }
        }

        private void DoAction()
        {
            //// TODO [vmaklai] Move this method's logic to the View Model

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

            if (this.ViewModel.ParametersPage.IsReplaceMode)
            {
                UIElement popupElement;
                Point? popupPoint;
                if (TryReplaceWithParameter(parameter, out popupElement, out popupPoint))
                {
                    UIHelper.ShowInfoPopup(
                        popupElement,
                        Properties.Resources.ReplacedWithParameterPopupText,
                        popupPoint);
                }

                RaiseActionExecuted(true);
                return;
            }

            Clipboard.SetText(parameter);
            UIHelper.ShowInfoPopup(this, Properties.Resources.CopiedToClipboardPopupText);
            RaiseActionExecuted(false);
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedNode();
        }

        private void ReplaceOrCopyButton_Click(object sender, RoutedEventArgs e)
        {
            DoAction();
        }

        private void ParametersPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ParametersPageViewModel.IdocTreeNodesViewPropertyName)
            {
                FocusSelectedNode();
            }
        }

        #endregion
    }
}