using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class ParametersPageViewModel : ViewModelBase
    {
        #region Constants and Fields

        private static readonly string IsSelectedPropertyName =
            Helper.GetPropertyName((IdocTreeNode obj) => obj.IsSelected);

        private readonly GeneratorControlViewModel _owner;
        private readonly List<ControlItem<RepositoryItem>> _idocItems;
        private readonly List<IdocTreeNode> _idocTreeNodes;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParametersPageViewModel"/> class.
        /// </summary>
        public ParametersPageViewModel(GeneratorControlViewModel owner)
        {
            #region Argument Check

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            #endregion

            _owner = owner;

            _idocItems = new List<ControlItem<RepositoryItem>>();
            this.IdocItemsView = CollectionViewSource.GetDefaultView(_idocItems);
            this.IdocItemsView.CurrentChanged += this.IdocItemsView_CurrentChanged;

            _idocTreeNodes = new List<IdocTreeNode>();
            this.IdocTreeNodesView = CollectionViewSource.GetDefaultView(_idocTreeNodes);
            this.IdocTreeNodesView.CurrentChanged += this.IdocTreeNodesView_CurrentChanged;
        }

        #endregion

        #region Public Properties

        public ICollectionView IdocItemsView
        {
            get;
            private set;
        }

        public ICollectionView IdocTreeNodesView
        {
            get;
            private set;
        }

        public bool IsReplaceOrCopyButtonEnabled
        {
            get
            {
                var selectedIdocTreeNode = GetSelectedIdocTreeNode();
                return selectedIdocTreeNode != null && selectedIdocTreeNode.Parent != null;
            }
        }

        #endregion

        #region Public Methods

        public override void Reset()
        {
            // Nothing to do
        }

        public void SetIdocItems(bool keepSelection, IEnumerable<RepositoryItem> items)
        {
            #region Argument Check

            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            #endregion

            var oldSelectedItem = keepSelection ? this.IdocItemsView.CurrentItem : null;

            _idocItems.Clear();
            _idocItems.AddRange(items.Select(obj => ControlItem.Create(obj, obj.Folder)));

            this.IdocItemsView.Refresh();

            if (oldSelectedItem != null)
            {
                this.IdocItemsView.MoveCurrentTo(oldSelectedItem);
            }

            RaisePropertyChanged(obj => obj.IdocItemsView);
        }

        public IdocTreeNode GetSelectedIdocTreeNode()
        {
            return GetSelectedIdocTreeNodeInternal(_idocTreeNodes);
        }

        #endregion

        #region Private Methods

        private static IdocTreeNode GetSelectedIdocTreeNodeInternal(IEnumerable<IdocTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.IsSelected)
                {
                    return node;
                }

                var selectedChild = GetSelectedIdocTreeNodeInternal(node.Children);
                if (selectedChild != null)
                {
                    return selectedChild;
                }
            }

            return null;
        }

        private void RaisePropertyChanged<T>(
            Expression<Func<ParametersPageViewModel, T>> propertyGetterExpression)
        {
            RaisePropertyChanged<ParametersPageViewModel, T>(propertyGetterExpression);
        }

        private void SubscribeToNode(IdocTreeNode node)
        {
            node.PropertyChanged += this.IdocTreeNode_PropertyChanged;
        }

        private void RefreshIdocTree()
        {
            DoRefreshIdocTree();

            this.IdocTreeNodesView.Refresh();
            RaisePropertyChanged(obj => obj.IdocTreeNodesView);
            RaisePropertyChanged(obj => obj.IsReplaceOrCopyButtonEnabled);
        }

        private void DoRefreshIdocTree()
        {
            _idocTreeNodes.Clear();

            var selectedRepositoryItem = this.IdocItemsView.CurrentItem as ControlItem<RepositoryItem>;
            if (selectedRepositoryItem == null
                || selectedRepositoryItem.Value == null
                || selectedRepositoryItem.Value.Count == 0)
            {
                return;
            }

            var xmlIdocFilePath = selectedRepositoryItem.Value.XmlIdocFiles.First();
            XDocument document;
            using (var stream = File.OpenRead(xmlIdocFilePath))
            {
                document = XDocument.Load(stream);
            }

            var segmentElements = document.XPathSelectElements("//*[@SEGMENT='1']");
            foreach (var segmentElement in segmentElements)
            {
                var segmentTreeNode = new IdocTreeNode(null) { Name = segmentElement.Name.LocalName };
                SubscribeToNode(segmentTreeNode);
                _idocTreeNodes.Add(segmentTreeNode);

                var fieldElements = segmentElement.Descendants();
                foreach (var fieldElement in fieldElements)
                {
                    var fieldTreeNode = new IdocTreeNode(segmentTreeNode)
                    {
                        Name = fieldElement.Name.LocalName,
                        Value = fieldElement.Value
                    };

                    SubscribeToNode(fieldTreeNode);
                    segmentTreeNode.Children.Add(fieldTreeNode);
                }
            }
        }

        private void IdocItemsView_CurrentChanged(object sender, EventArgs eventArgs)
        {
            RefreshIdocTree();
        }

        private void IdocTreeNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == IsSelectedPropertyName)
            {
                RaisePropertyChanged(obj => obj.IsReplaceOrCopyButtonEnabled);
            }
        }

        private void IdocTreeNodesView_CurrentChanged(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(obj => obj.IsReplaceOrCopyButtonEnabled);
        }

        #endregion
    }
}