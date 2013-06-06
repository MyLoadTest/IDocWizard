using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using HP.LR.VuGen.ServiceCore.Data.ProjectSystem;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class WizardPageViewModel : ViewModelBase
    {
        #region Constants and Fields

        private string _definitionFilePath;
        private string _exampleFilePath;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WizardPageViewModel"/> class.
        /// </summary>
        public WizardPageViewModel()
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
                RaisePropertyChanged(obj => obj.IsCreateEnabled);
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
                RaisePropertyChanged(obj => obj.IsCreateEnabled);
            }
        }

        public bool IsCreateEnabled
        {
            get
            {
                return !this.DefinitionFilePath.IsNullOrWhiteSpace()
                    && !this.ExampleFilePath.IsNullOrWhiteSpace();
            }
        }

        #endregion

        #region Public Methods

        public override void Reset()
        {
            this.DefinitionFilePath = string.Empty;
            this.ExampleFilePath = string.Empty;
        }

        public void GenerateAction(IActionScriptItem actionItem)
        {
            #region Argument Check

            if (actionItem == null)
            {
                throw new ArgumentNullException("actionItem");
            }

            #endregion

            var definition = SapIDocDefinition.LoadHeader(this.DefinitionFilePath);
            var idocText = File.ReadAllText(this.ExampleFilePath);
            var doc = new SapIDoc(definition, idocText);
            var actionContents = doc.GetVuGenActionContents();

            File.WriteAllText(actionItem.FullFileName, actionContents, Encoding.Default);
        }

        #endregion

        #region Private Methods

        private void RaisePropertyChanged<T>(
            Expression<Func<WizardPageViewModel, T>> propertyGetterExpression)
        {
            RaisePropertyChanged<WizardPageViewModel, T>(propertyGetterExpression);
        }

        #endregion
    }
}