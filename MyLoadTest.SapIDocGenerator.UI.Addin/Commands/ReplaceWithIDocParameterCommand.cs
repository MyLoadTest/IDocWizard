using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Gui;
using MyLoadTest.SapIDocGenerator.UI.Addin.Pads;
using MyLoadTest.SapIDocGenerator.UI.Controls;

namespace MyLoadTest.SapIDocGenerator.UI.Addin.Commands
{
    public sealed class ReplaceWithIDocParameterCommand : AbstractMenuCommand
    {
        #region Public Methods

        /// <summary>
        ///     Invokes the command.
        /// </summary>
        public override void Run()
        {
            if (ShowParametersPopup())
            {
                return;
            }

            var wizardPad = WizardPad.ShowInWorkbench();
            if (wizardPad != null)
            {
                wizardPad.InnerControl.ActivateParameters();
            }
        }

        #endregion

        #region Private Methods

        private static bool DoShowParametersPopup(TextArea textArea, Point popupPoint)
        {
            #region Argument Check

            if (textArea == null)
            {
                throw new ArgumentNullException("textArea");
            }

            #endregion

            var text = textArea.Selection.GetText(textArea.Document);
            if (text.IsNullOrEmpty())
            {
                return false;
            }

            var wizardPad = WizardPad.FindPad();
            if (wizardPad == null)
            {
                return false;
            }

            const int Height = 400;
            var parametersPage = new ParametersPageControl
            {
                Width = 600,
                Height = Height,
                MainGridMaxHeight = Height - 20,
                ViewModel =
                {
                    ParametersPage = { IsReplaceMode = true, AutoFocusedValue = text }
                }
            };

            // Must be set after the control is created and initialized
            parametersPage.ViewModel.ImportPage.RepositoryPath =
                wizardPad.InnerControl.ViewModel.ImportPage.RepositoryPath;
            parametersPage.ViewModel.ParametersPage.SetSelectedIdocItem(
                wizardPad.InnerControl.ViewModel.ParametersPage.GetSelectedIdocItem());

            var popupContent = new Grid
            {
                Background = SystemColors.ControlBrush,
                Children =
                {
                    new Border
                    {
                        BorderThickness = new Thickness(2d),
                        BorderBrush = SystemColors.ActiveBorderBrush,
                        Child = parametersPage
                    }
                }
            };

            var popup = new KeyboardFriendlyPopup
            {
                IsOpen = false,
                StaysOpen = false,
                AllowsTransparency = true,
                Placement = PlacementMode.Relative,
                PlacementTarget = textArea,
                HorizontalOffset = popupPoint.X,
                VerticalOffset = popupPoint.Y,
                PopupAnimation = PopupAnimation.None,
                Focusable = false,
                Opacity = 0d,
                Child = popupContent
            };

            popup.PreviewMouseLeftButtonDown +=
                (sender, e) =>
                {
                    if (e.ClickCount == 2)
                    {
                        parametersPage.PerformAction();
                    }
                };

            FocusManager.SetFocusedElement(popup, parametersPage.ParameterTreeViewReference);

            popup.Closed += (sender, e) => textArea.Focus();

            popup.PreviewKeyDown +=
                (sender, e) =>
                {
                    switch (e.Key)
                    {
                        case Key.Escape:
                            popup.IsOpen = false;
                            break;

                        case Key.Enter:
                            parametersPage.PerformAction();
                            break;
                    }
                };

            parametersPage.ActionExecuted += (sender, e) => popup.IsOpen = false;

            popup.IsOpen = true;
            return true;
        }

        private static bool ShowParametersPopup()
        {
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
            if (viewContent == null)
            {
                return false;
            }

            var textArea = viewContent.InitiallyFocusedControl as TextArea;
            if (textArea == null || textArea.TextView == null)
            {
                return false;
            }

            textArea.Caret.BringCaretToView();

            var visualPosition = textArea.TextView.GetVisualPosition(
                new TextViewPosition(textArea.Caret.Line, textArea.Caret.Column),
                VisualYPosition.LineBottom);
            var popupPoint = visualPosition - textArea.TextView.ScrollOffset;

            return DoShowParametersPopup(textArea, popupPoint);
        }

        #endregion
    }
}