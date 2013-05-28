using System;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class RepositoryItem
    {
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
    }
}