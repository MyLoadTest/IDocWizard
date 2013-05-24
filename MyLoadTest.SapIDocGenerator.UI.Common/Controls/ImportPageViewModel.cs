using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Data;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class ImportPageViewModel : ViewModelBase
    {
        #region Constants and Fields

        private readonly ObservableCollection<RepositoryItem> _repositoryItemsDirect;
        private string _repositoryPath;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportPageViewModel"/> class.
        /// </summary>
        public ImportPageViewModel()
        {
            _repositoryItemsDirect = new ObservableCollection<RepositoryItem>();
            this.RepositoryItems = new ReadOnlyObservableCollection<RepositoryItem>(_repositoryItemsDirect);

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
                RefreshRepositoryItems();
            }
        }

        public bool IsRepositoryPathSelected
        {
            get
            {
                return !this.RepositoryPath.IsNullOrWhiteSpace();
            }
        }

        public ReadOnlyObservableCollection<RepositoryItem> RepositoryItems
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override void Reset()
        {
            this.RepositoryPath = string.Empty;
            RefreshRepositoryItems();
        }

        public void RefreshRepositoryItems()
        {
            _repositoryItemsDirect.Clear();

            if (this.RepositoryPath.IsNullOrWhiteSpace())
            {
                return;
            }

            try
            {
                var directories = Directory.GetDirectories(this.RepositoryPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var directory in directories)
                {
                    if (!Directory.GetFiles(directory, "*.h", SearchOption.TopDirectoryOnly).Any())
                    {
                        continue;
                    }

                    var count = Directory.GetFiles(directory, "*.xml", SearchOption.TopDirectoryOnly).Length;

                    var item = new RepositoryItem { Folder = Path.GetFileName(directory), Count = count };
                    _repositoryItemsDirect.Add(item);
                }
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                Helper.ShowErrorBox(
                    null,
                    ex,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Error reading repository '{0}'.",
                        this.RepositoryPath));
            }
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