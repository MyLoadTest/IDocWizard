using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public class KeyboardFriendlyPopup : Popup
    {
        #region Protected Methods

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            FixFocus();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (this.IsKeyboardFocusWithin)
            {
                return;
            }

            if (this.IsOpen)
            {
                FixFocus();
            }
        }

        #endregion

        #region Private Methods

        private void FixFocus()
        {
            if (this.Child == null)
            {
                return;
            }

            this.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        #endregion
    }
}