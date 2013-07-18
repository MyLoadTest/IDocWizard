using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Public Methods

        public abstract void Reset(bool restoreSettings);

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Methods

        protected void RaisePropertyChanged<TViewModel, T>(
            Expression<Func<TViewModel, T>> propertyGetterExpression)
            where TViewModel : ViewModelBase
        {
            #region Argument Check

            if (propertyGetterExpression == null)
            {
                throw new ArgumentNullException("propertyGetterExpression");
            }

            #endregion

            if (this.PropertyChanged == null)
            {
                return;
            }

            var propertyName = Helper.GetPropertyName(propertyGetterExpression);
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}