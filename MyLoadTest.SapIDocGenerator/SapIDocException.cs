using System;
using System.Linq;
using System.Runtime.Serialization;

namespace MyLoadTest.SapIDocGenerator
{
    [Serializable]
    public sealed class SapIDocException : Exception
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SapIDocException"/> class.
        /// </summary>
        public SapIDocException()
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SapIDocException"/> class.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        public SapIDocException(string message)
            : base(message)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SapIDocException"/> class.
        /// </summary>
        /// <param name="message">
        ///     The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a <b>null</b> reference
        ///     (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public SapIDocException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Prevents a default instance of the <see cref="SapIDocException"/> class from being created.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        private SapIDocException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Nothing to do
        }

        #endregion
    }
}