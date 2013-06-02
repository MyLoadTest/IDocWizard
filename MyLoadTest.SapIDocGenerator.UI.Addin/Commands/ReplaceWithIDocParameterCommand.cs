using System;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.Core;
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
            var wizardPad = WizardPad.ShowInWorkbench();
            if (wizardPad != null)
            {
                wizardPad.InnerControl.ActivateTab(GeneratorControlTab.Parameters);
            }
        }

        #endregion
    }
}