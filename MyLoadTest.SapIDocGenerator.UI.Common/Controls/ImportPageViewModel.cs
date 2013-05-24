using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class ImportPageViewModel : ViewModelBase
    {
        #region Constants and Fields

        private string _repositoryPath;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportPageViewModel"/> class.
        /// </summary>
        public ImportPageViewModel()
        {
            Reset();
        }

        #endregion

        #region Public Properties

        public string RepositoryPath
        {
            [DebuggerNonUserCode]
            get
            {
                return _repositoryPath;
            }

            set
            {
                if (value == _repositoryPath)
                {
                    return;
                }

                _repositoryPath = value;
                RaisePropertyChanged(obj => obj.RepositoryPath);
                RaisePropertyChanged(obj => obj.IsRepositoryPathSelected);
            }
        }

        public bool IsRepositoryPathSelected
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.RepositoryPath);
            }
        }

        #endregion

        #region Public Methods

        public override void Reset()
        {
            this.RepositoryPath = string.Empty;
        }

        #endregion

        #region Private Methods

        private void RaisePropertyChanged<T>(
            Expression<Func<ImportPageViewModel, T>> propertyGetterExpression)
        {
            RaisePropertyChanged<ImportPageViewModel, T>(propertyGetterExpression);
        }

        #endregion
    }
}