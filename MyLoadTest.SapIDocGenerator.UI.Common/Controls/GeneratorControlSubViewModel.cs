using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public abstract class GeneratorControlSubViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GeneratorControlSubViewModel"/> class.
        /// </summary>
        protected GeneratorControlSubViewModel(GeneratorControlViewModel owner)
        {
            #region Argument Check

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            #endregion

            this.Owner = owner;
        }

        #endregion

        #region Public Properties

        protected GeneratorControlViewModel Owner
        {
            get;
            private set;
        }

        #endregion
    }
}