using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Data;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class ParametersPageViewModel : ViewModelBase
    {
        #region Constants and Fields

        private readonly List<ControlItem<string>> _idocItems;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParametersPageViewModel"/> class.
        /// </summary>
        public ParametersPageViewModel()
        {
            _idocItems = new List<ControlItem<string>>();
            this.IdocItems = CollectionViewSource.GetDefaultView(_idocItems);

            Reset();
        }

        #endregion

        #region Public Properties

        public ICollectionView IdocItems
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override void Reset()
        {
            // Nothing to do
        }

        #endregion

        #region Private Methods

        private void RaisePropertyChanged<T>(
            Expression<Func<ParametersPageViewModel, T>> propertyGetterExpression)
        {
            RaisePropertyChanged<ParametersPageViewModel, T>(propertyGetterExpression);
        }

        #endregion
    }
}