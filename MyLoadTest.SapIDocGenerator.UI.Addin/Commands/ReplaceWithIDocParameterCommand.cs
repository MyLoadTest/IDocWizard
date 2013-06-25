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
            ////if (DoTest())
            ////{
            ////    return;
            ////}

            var wizardPad = WizardPad.ShowInWorkbench();
            if (wizardPad != null)
            {
                wizardPad.InnerControl.ActivateParameters();
            }
        }

        #endregion

        private static bool DoShowTestPopup(TextArea textArea, Point? popupPoint = null)
        {
            #region Argument Check

            if (textArea == null)
            {
                throw new ArgumentNullException("textArea");
            }

            #endregion

            var text = textArea.Selection.GetText(textArea.Document);
            if (text.IsNullOrWhiteSpace())
            {
                return false;
            }

            var popupTextBlock = new TextBox
            {
                Text = text,
                Background = SystemColors.HighlightBrush,
                Foreground = SystemColors.HighlightTextBrush,
                FontSize = 20,
                Padding = new Thickness(5d)
            };

            var popupContent = new Grid
            {
                Background = SystemColors.ControlBrush,
                Children =
                {
                    new Border
                    {
                        BorderThickness = new Thickness(2d),
                        BorderBrush = SystemColors.ActiveBorderBrush,
                        Child = popupTextBlock
                    }
                }
            };

            var popup = new KeyboardFriendlyPopup
            {
                IsOpen = false,
                StaysOpen = false,
                AllowsTransparency = true,
                Placement = popupPoint.HasValue ? PlacementMode.Relative : PlacementMode.Center,
                PlacementTarget = textArea,
                HorizontalOffset = popupPoint.HasValue ? popupPoint.Value.X : 0,
                VerticalOffset = popupPoint.HasValue ? popupPoint.Value.Y : 0,
                PopupAnimation = PopupAnimation.None,
                Focusable = true,
                Opacity = 0d,
                Child = popupContent
            };

            popup.Closed += (sender, e) => textArea.Focus();

            popup.MouseUp += (sender, e) => popup.IsOpen = false;

            popup.PreviewKeyDown +=
                (sender, e) =>
                {
                    switch (e.Key)
                    {
                        case Key.Escape:
                            popup.IsOpen = false;
                            break;

                        case Key.Enter:
                            textArea.Selection.ReplaceSelectionWithText(textArea, popupTextBlock.Text);
                            popup.IsOpen = false;
                            break;
                    }
                };

            popup.IsOpen = true;

            return true;
        }

        private static bool DoTest()
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

            return DoShowTestPopup(textArea, popupPoint);
        }
    }
}