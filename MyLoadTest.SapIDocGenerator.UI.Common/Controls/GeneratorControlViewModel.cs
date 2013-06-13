using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class GeneratorControlViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GeneratorControlViewModel"/> class.
        /// </summary>
        public GeneratorControlViewModel()
        {
            this.ImportPage = new ImportPageViewModel(this);
            this.WizardPage = new WizardPageViewModel(this);
            this.ParametersPage = new ParametersPageViewModel(this);

            Reset();
        }

        #endregion

        #region Public Properties

        public WizardPageViewModel WizardPage
        {
            get;
            private set;
        }

        public ImportPageViewModel ImportPage
        {
            get;
            private set;
        }

        public ParametersPageViewModel ParametersPage
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override void Reset()
        {
            this.WizardPage.Reset();
            this.ImportPage.Reset();
            this.ParametersPage.Reset();
        }

        #endregion
    }
}