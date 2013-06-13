using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class IdocTreeNode : INotifyPropertyChanged
    {
        #region Constants and Fields

        private string _name;
        private string _value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdocTreeNode"/> class.
        /// </summary>
        public IdocTreeNode()
        {
            this.Children = new ObservableCollection<IdocTreeNode>();
        }

        #endregion

        #region Public Properties

        public string Name
        {
            [DebuggerNonUserCode]
            get
            {
                return _name;
            }

            set
            {
                if (value == _name)
                {
                    return;
                }

                _name = value;
                RaisePropertyChanged(obj => obj.Name);
            }
        }

        public string Value
        {
            [DebuggerNonUserCode]
            get
            {
                return _value;
            }

            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;
                RaisePropertyChanged(obj => obj.Value);
                RaisePropertyChanged(obj => obj.HasValue);
            }
        }

        public bool HasValue
        {
            get
            {
                return this.Value != null;
            }
        }

        public ObservableCollection<IdocTreeNode> Children
        {
            get;
            private set;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Methods

        private void RaisePropertyChanged<T>(
            Expression<Func<IdocTreeNode, T>> propertyGetterExpression)
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