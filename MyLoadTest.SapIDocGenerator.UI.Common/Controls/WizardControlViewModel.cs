using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class WizardControlViewModel : ViewModelBase
    {
        #region Constants and Fields

        private string _definitionFilePath;
        private string _exampleFilePath;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WizardControlViewModel"/> class.
        /// </summary>
        public WizardControlViewModel()
        {
            Reset();
        }

        #endregion

        #region Public Properties

        public string DefinitionFilePath
        {
            [DebuggerNonUserCode]
            get
            {
                return _definitionFilePath;
            }

            set
            {
                if (value == _definitionFilePath)
                {
                    return;
                }

                _definitionFilePath = value;
                RaisePropertyChanged(obj => obj.DefinitionFilePath);
                RaisePropertyChanged(obj => obj.IsInputValid);
            }
        }

        public string ExampleFilePath
        {
            [DebuggerNonUserCode]
            get
            {
                return _exampleFilePath;
            }

            set
            {
                if (value == _exampleFilePath)
                {
                    return;
                }

                _exampleFilePath = value;
                RaisePropertyChanged(obj => obj.ExampleFilePath);
                RaisePropertyChanged(obj => obj.IsInputValid);
            }
        }

        public bool IsInputValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.DefinitionFilePath)
                    && !string.IsNullOrWhiteSpace(this.ExampleFilePath);
            }
        }

        #endregion

        #region Public Methods

        public void Reset()
        {
            this.DefinitionFilePath = string.Empty;
            this.ExampleFilePath = string.Empty;
        }

        #endregion

        #region Private Methods

        private void RaisePropertyChanged<T>(
            Expression<Func<WizardControlViewModel, T>> propertyGetterExpression)
        {
            RaisePropertyChanged<WizardControlViewModel, T>(propertyGetterExpression);
        }

        #endregion
    }
}