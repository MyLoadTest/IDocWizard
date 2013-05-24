using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

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
            this.ImportPage = new ImportPageViewModel();
            this.WizardPage = new WizardPageViewModel();

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

        #endregion

        #region Public Methods

        public override void Reset()
        {
            this.WizardPage.Reset();
            this.ImportPage.Reset();
        }

        #endregion

        #region Private Methods

        private void RaisePropertyChanged<T>(
            Expression<Func<GeneratorControlViewModel, T>> propertyGetterExpression)
        {
            RaisePropertyChanged<GeneratorControlViewModel, T>(propertyGetterExpression);
        }

        #endregion
    }
}