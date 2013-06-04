using System;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class RepositoryItem : IEquatable<RepositoryItem>
    {
        #region Public Properties

        public string Folder
        {
            get;
            internal set;
        }

        public int Count
        {
            get;
            internal set;
        }

        public string DefinitionFilePath
        {
            get;
            internal set;
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            return Equals(obj as RepositoryItem);
        }

        public override int GetHashCode()
        {
            return this.Folder == null ? 0 : this.Folder.GetHashCode();
        }

        #endregion

        #region IEquatable<RepositoryItem> Members

        public bool Equals(RepositoryItem other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(other.Folder, this.Folder);
        }

        #endregion
    }
}