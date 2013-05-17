using System;
using System.Linq;
using System.Runtime.Serialization;

namespace MyLoadTest.SapIDocGenerator
{
    [Serializable]
    public sealed class SapIDocDefinitionException : Exception
    {
        #region Constructors

        public SapIDocDefinitionException()
        {
            // Nothing to do
        }

        public SapIDocDefinitionException(string message)
            : base(message)
        {
            // Nothing to do
        }

        public SapIDocDefinitionException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Nothing to do
        }

        private SapIDocDefinitionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Nothing to do
        }

        #endregion
    }
}