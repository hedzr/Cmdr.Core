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
}