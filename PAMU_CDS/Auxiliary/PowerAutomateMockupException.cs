using System;
using System.Runtime.Serialization;

namespace PAMU_CDS.Auxiliary
{
    public class PowerAutomateException : Exception
    {
        public PowerAutomateException()
        {
        }

        public PowerAutomateException(string message) : base(message)
        {
        }

        public PowerAutomateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PowerAutomateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}