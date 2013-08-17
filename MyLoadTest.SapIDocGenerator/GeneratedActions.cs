using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator
{
    public sealed class GeneratedActions
    {
        #region Constructors

        internal GeneratedActions(string mainActionContents, string initActionContents, string endActionContents)
        {
            #region Argument Check

            if (mainActionContents == null)
            {
                throw new ArgumentNullException("mainActionContents");
            }

            if (initActionContents == null)
            {
                throw new ArgumentNullException("initActionContents");
            }

            if (endActionContents == null)
            {
                throw new ArgumentNullException("endActionContents");
            }

            #endregion

            this.MainActionContents = mainActionContents;
            this.InitActionContents = initActionContents;
            this.EndActionContents = endActionContents;
        }

        #endregion

        #region Public Properties

        public string MainActionContents
        {
            get;
            private set;
        }

        public string InitActionContents
        {
            get;
            private set;
        }

        public string EndActionContents
        {
            get;
            private set;
        }

        #endregion
    }
}