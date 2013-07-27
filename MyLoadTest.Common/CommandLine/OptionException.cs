using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace MyLoadTest.CommandLine
{
    [Serializable]
    public sealed class OptionException : Exception
    {
        #region Constants and Fields

        private const string OptionNameKey = "OptionName";

        #endregion

        #region Constructors

        internal OptionException(string message, string optionName)
            : base(message)
        {
            this.OptionName = optionName;
        }

        internal OptionException(string message, string optionName, Exception innerException)
            : base(message, innerException)
        {
            this.OptionName = optionName;
        }

        private OptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.OptionName = info.GetString(OptionNameKey);
        }

        #endregion

        #region Public Properties

        public string OptionName
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(OptionNameKey, this.OptionName);
        }

        #endregion
    }
}