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
using MyLoadTest.Configuration;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class ImportPageViewModel : GeneratorControlSubViewModel
    {
        #region Constants and Fields

        private static readonly string[] EmptyDirectoryArray = new string[0];

        private readonly List<RepositoryItem> _repositoryItems;
        private string _repositoryPath;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportPageViewModel"/> class.
        /// </summary>
        public ImportPageViewModel(GeneratorControlViewModel owner)
            : base(owner)
        {
            _repositoryItems = new List<RepositoryItem>();

            this.RepositoryItemsView = CollectionViewSource.GetDefaultView(_repositoryItems);
            this.RepositoryItemsView.SortDescriptions.Add(
                new SortDescription(
                    Helper.GetPropertyName((RepositoryItem obj) => obj.Folder),
                    ListSortDirection.Ascending));
            this.RepositoryItemsView.CurrentChanged += this.RepositoryItemsView_CurrentChanged;
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

                SetRepositoryPathInternal(value, true);
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

        public bool IsParametersTabAvailable
        {
            get
            {
                return this.IsRepositoryPathSelected;
            }
        }

        #endregion

        #region Public Methods

        public override void Reset(bool restoreSettings)
        {
            if (!restoreSettings)
            {
                return;
            }

            SetRepositoryPathInternal(SettingManager.Instance.RepositoryPath ?? string.Empty, false);
        }

        public void RefreshRepositoryItems(bool keepSelection = true)
        {
            var oldSelectedItem = keepSelection ? this.RepositoryItemsView.CurrentItem : null;

            _repositoryItems.Clear();
            try
            {
                var directories = this.RepositoryPath.IsNullOrWhiteSpace()
                    ? EmptyDirectoryArray
                    : Directory.GetDirectories(this.RepositoryPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var directory in directories)
                {
                    string definitionFilePath;
                    try
                    {
                        definitionFilePath = Directory
                            .GetFiles(directory, Constants.DefinitionFileMask, SearchOption.TopDirectoryOnly)
                            .FirstOrDefault();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Logger.ErrorFormat(
                            ex,
                            "Error reading repository \"{0}\" at \"{1}\".",
                            this.RepositoryPath,
                            directory);

                        continue;
                    }

                    if (definitionFilePath.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    var xmlIdocFiles = Directory.GetFiles(
                        directory,
                        Constants.XmlIdocFileMask,
                        SearchOption.TopDirectoryOnly);

                    var item = new RepositoryItem
                    {
                        Folder = Path.GetFileName(directory),
                        XmlIdocFiles = xmlIdocFiles,
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
                        "Error reading repository \"{0}\"",
                        this.RepositoryPath));
            }

            this.RepositoryItemsView.Refresh();

            if (oldSelectedItem != null)
            {
                this.RepositoryItemsView.MoveCurrentTo(oldSelectedItem);
            }

            this.Owner.ParametersPage.SetIdocItems(keepSelection, _repositoryItems);
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

        private void SetRepositoryPathInternal(string value, bool saveSetting)
        {
            _repositoryPath = value;

            RaisePropertyChanged(obj => obj.RepositoryPath);
            RaisePropertyChanged(obj => obj.IsRepositoryPathSelected);
            RaisePropertyChanged(obj => obj.ShouldImportButtonBeEnabled);
            RaisePropertyChanged(obj => obj.IsParametersTabAvailable);

            RefreshRepositoryItems();

            if (saveSetting)
            {
                SettingManager.Instance.RepositoryPath = _repositoryPath;
            }
        }

        private void RepositoryItemsView_CurrentChanged(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(obj => obj.ShouldImportButtonBeEnabled);
        }

        #endregion
    }
}