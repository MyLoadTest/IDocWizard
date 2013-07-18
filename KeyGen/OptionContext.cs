using System;
using System.Linq;

namespace MyLoadTest.IDoc.KeyGen
{
    public class OptionContext
    {
        #region Constants and Fields

        private readonly OptionValueCollection _optionValues;

        #endregion

        #region Constructors

        public OptionContext(OptionSet optionSet)
        {
            #region Argument Check

            if (optionSet == null)
            {
                throw new ArgumentNullException("optionSet");
            }

            #endregion

            this.OptionSet = optionSet;
            _optionValues = new OptionValueCollection(this);
        }

        #endregion

        #region Public Properties

        public Option Option
        {
            get;
            set;
        }

        public string OptionName
        {
            get;
            set;
        }

        public int OptionIndex
        {
            get;
            set;
        }

        public OptionSet OptionSet
        {
            get;
            private set;
        }

        public OptionValueCollection OptionValues
        {
            get
            {
                return _optionValues;
            }
        }

        #endregion
    }
}