using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Data;
using System.Xml.Linq;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class ImportPageViewModel : ViewModelBase
    {
        #region Constants and Fields

        private readonly List<RepositoryItem> _repositoryItems;
        private string _repositoryPath;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportPageViewModel"/> class.
        /// </summary>
        public ImportPageViewModel()
        {
            _repositoryItems = new List<RepositoryItem>();

            this.RepositoryItemsView = CollectionViewSource.GetDefaultView(_repositoryItems);
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

        public void RefreshRepositoryItems(bool keepSelection = true)
        {
            var oldSelectedItem = keepSelection ? this.RepositoryItemsView.CurrentItem : null;

            _repositoryItems.Clear();

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

                    _repositoryItems.Add(item);
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

            this.RepositoryItemsView.Refresh();

            if (oldSelectedItem != null)
            {
                this.RepositoryItemsView.MoveCurrentTo(oldSelectedItem);
            }
        }

        public void CreateNewType(string filePath)
        {
            #region Argument Check

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    @"The value can be neither empty or whitespace-only string nor null.",
                    "filePath");
            }

            #endregion

            var definition = SapIDocDefinition.LoadHeader(filePath);

            var path = Path.Combine(this.RepositoryPath, definition.Name);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var destinationFilePath = Path.Combine(path, Path.GetFileName(filePath).EnsureNotNull());
            File.Copy(filePath, destinationFilePath, true);
        }

        public void ImportIdocFiles(RepositoryItem repositoryItem, IEnumerable<string> filePaths)
        {
            #region Argument Check

            if (repositoryItem == null)
            {
                throw new ArgumentNullException("repositoryItem");
            }

            if (filePaths == null)
            {
                throw new ArgumentNullException("filePaths");
            }

            #endregion

            var definition = SapIDocDefinition.LoadHeader(repositoryItem.DefinitionFilePath);
            foreach (var filePath in filePaths)
            {
                var contents = File.ReadAllText(filePath);
                var doc = new SapIDoc(definition, contents);
                var resultingFileContents = doc.GetXml().ToString(SaveOptions.None);

                var resultingFilePath = Path.Combine(
                    this.RepositoryPath,
                    repositoryItem.Folder,
                    string.Format(CultureInfo.InvariantCulture, "{0}.txt", doc.Number));

                File.WriteAllText(resultingFilePath, resultingFileContents);
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