using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using MyLoadTest.SapIDocGenerator.UI.Addin.Pads;

namespace MyLoadTest.SapIDocGenerator.UI.Addin.Commands
{
    /// <summary>
    ///     Represents the command that is executed automatically when IDE starts.
    /// </summary>
    public sealed class StartupCommand : AbstractMenuCommand
    {
        #region Public Methods

        /// <summary>
        ///     Invokes the command.
        /// </summary>
        public override void Run()
        {
            WorkbenchSingleton.WorkbenchCreated += this.WorkbenchSingleton_WorkbenchCreated;
       }

        #endregion

        #region Private Methods

        private void WorkbenchSingleton_WorkbenchCreated(object sender, EventArgs e)
        {
            WorkbenchSingleton.Workbench.MainWindow.IsVisibleChanged += this.MainWindow_IsVisibleChanged;
            WorkbenchSingleton.Workbench.MainWindow.Activated += this.MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, EventArgs eventArgs)
        {
            WorkbenchSingleton.Workbench.MainWindow.Activated -= this.MainWindow_Activated;

            WizardPad.ShowInWorkbench();
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!WorkbenchSingleton.Workbench.MainWindow.IsVisible)
            {
                return;
            }

            WorkbenchSingleton.Workbench.MainWindow.IsVisibleChanged -= this.MainWindow_IsVisibleChanged;

            WizardPad.ShowInWorkbench();
        }

        #endregion
    }
}