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

            this.RepositoryItemsView = CollectionViewSource.GetDefaultView(this.RepositoryItems);
            this.RepositoryItemsView.SortDescriptions.Add(
                new SortDescription(
                    Helper.GetPropertyName((RepositoryItem obj) => obj.Folder),
                    ListSortDirection.Ascending));
            this.RepositoryItemsView.CurrentChanged += this.RepositoryItemsView_CurrentChanged;

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

        public ICollectionView RepositoryItemsView
        {
            get;
            private set;
        }

        public bool ShouldImportButtonBeEnabled
        {
            get
            {
                return this.IsRepositoryPathSelected && this.RepositoryItemsView.CurrentItem != null;
            }
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
                    var definitionFilePath = Directory
                        .GetFiles(directory, "*.h", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault();
                    if (definitionFilePath.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    var count = Directory.GetFiles(directory, "*.txt", SearchOption.TopDirectoryOnly).Length;

                    var item = new RepositoryItem
                    {
                        Folder = Path.GetFileName(directory),
                        Count = count,
                        DefinitionFilePath = definitionFilePath
                    };

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

        private void RepositoryItemsView_CurrentChanged(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(obj => obj.ShouldImportButtonBeEnabled);
        }

        #endregion
    }
}