#nullable enable
using System.Runtime.Serialization;

namespace HzNS.Cmdr.Exception
{
    public class BaseException : System.Exception
    {
        public BaseException()
        {
        }

        protected BaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BaseException(string? message) : base(message)
        {
        }

        public BaseException(string? message, System.Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class CmdrException : BaseException
    {
        public CmdrException()
        {
        }

        protected CmdrException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CmdrException(string? message) : base(message)
        {
        }

        public CmdrException(string? message, System.Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class WantHelpScreenException : CmdrException
    {
    }
}