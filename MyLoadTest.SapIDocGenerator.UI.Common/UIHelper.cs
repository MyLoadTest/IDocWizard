using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace MyLoadTest.SapIDocGenerator.UI
{
    public static class UIHelper
    {
        #region Public Methods

        public static void ShowInfoPopup(UIElement popupElement, string text, Point? popupPoint = null)
        {
            #region Argument Check

            if (popupElement == null)
            {
                throw new ArgumentNullException("popupElement");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    @"The value can be neither empty or whitespace-only string nor null.",
                    "text");
            }

            #endregion

            var popupTextBlock = new TextBlock
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

            var popup = new Popup
            {
                IsOpen = false,
                StaysOpen = false,
                AllowsTransparency = true,
                Placement = popupPoint.HasValue ? PlacementMode.Relative : PlacementMode.Center,
                PlacementTarget = popupElement,
                HorizontalOffset = popupPoint.HasValue ? popupPoint.Value.X : 0,
                VerticalOffset = popupPoint.HasValue ? popupPoint.Value.Y : 0,
                PopupAnimation = PopupAnimation.None,
                Focusable = false,
                Opacity = 0d,
                Child = popupContent
            };

            popup.MouseUp += (sender, e) => popup.IsOpen = false;

            var fadeInAnimation = new DoubleAnimation(0d, 1d, new Duration(TimeSpan.FromSeconds(0.5d)));
            Storyboard.SetTarget(fadeInAnimation, popupContent);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));

            var fadeOutAnimation = new DoubleAnimation(1d, 0d, new Duration(TimeSpan.FromSeconds(0.5d)))
            {
                BeginTime = TimeSpan.FromSeconds(1.5d)
            };

            Storyboard.SetTarget(fadeOutAnimation, popupContent);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));

            var storyboard = new Storyboard { Children = { fadeInAnimation, fadeOutAnimation } };
            storyboard.Completed += (sender, args) => popup.IsOpen = false;

            popup.Opened += (sender, e) => popup.BeginStoryboard(storyboard);

            popup.IsOpen = true;
        }

        #endregion
    }
}