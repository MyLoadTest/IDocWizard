using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms.Integration;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using MyLoadTest.SapIDocGenerator.UI.Controls;

namespace MyLoadTest.SapIDocGenerator.UI.Addin.Pads
{
    public sealed class WizardPad : AbstractPadContent
    {
        #region Fields

        private readonly ElementHost _nativeControl;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WizardPad"/> class.
        /// </summary>
        public WizardPad()
        {
            this.InnerControl = new GeneratorControl();

            _nativeControl = new ElementHost
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Child = this.InnerControl
            };
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the <see cref="System.Windows.Forms.Control"/> representing the pad.
        /// </summary>
        public override object Control
        {
            [DebuggerStepThrough]
            get
            {
                return _nativeControl;
            }
        }

        /// <summary>
        ///     Gets the initially focused control.
        /// </summary>
        public override object InitiallyFocusedControl
        {
            [DebuggerStepThrough]
            get
            {
                return _nativeControl;
            }
        }

        public GeneratorControl InnerControl
        {
            get;
            private set;
        }

        public bool IsDisposed
        {
            [DebuggerNonUserCode]
            get
            {
                return _nativeControl.IsDisposed;
            }
        }

        #endregion

        #region Public Methods

        public static PadDescriptor FindPadDescriptor()
        {
            return WorkbenchSingleton.Workbench.GetPad(typeof(WizardPad));
        }

        public static WizardPad FindPad()
        {
            var pad = FindPadDescriptor();
            return pad == null ? null : pad.PadContent as WizardPad;
        }

        public static WizardPad ShowInWorkbench()
        {
            var padDescriptor = FindPadDescriptor();
            if (padDescriptor != null)
            {
                padDescriptor.CreatePad();
                DoActivate(padDescriptor);
            }

            return FindPad();
        }

        public void Activate()
        {
            if (this.IsDisposed)
            {
                return;
            }

            var padDescriptor = this.PadDescriptor;
            if (padDescriptor == null)
            {
                ShowInWorkbench();
                return;
            }

            DoActivate(padDescriptor);
        }

        public override void Dispose()
        {
            base.Dispose();

            _nativeControl.Dispose();
        }

        #endregion

        #region Private Methods

        private static void DoActivate(PadDescriptor padDescriptor)
        {
            var workbenchLayout = WorkbenchSingleton.Workbench.WorkbenchLayout;
            workbenchLayout.ShowPad(padDescriptor);
            workbenchLayout.ActivatePad(padDescriptor);

            padDescriptor.BringPadToFront();
        }

        #endregion
    }
}