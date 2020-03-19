#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Exception
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
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

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
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

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DuplicationFlagCharException : CmdrException
    {
        public DuplicationFlagCharException(bool isShort, string ch, IFlag flag, ICommand owner)
        {
            IsShort = isShort;
            Char = ch;
            Flag = flag;
            Owner = owner;
        }

        public bool IsShort { get; internal set; }
        public string Char { get; internal set; }
        public IFlag Flag { get; internal set; }
        public ICommand Owner { get; internal set; }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DuplicationCommandCharException : CmdrException
    {
        public DuplicationCommandCharException(bool isShort, string ch, ICommand cmd)
        {
            IsShort = isShort;
            Char = ch;
            Cmd = cmd;
        }

        public bool IsShort { get; internal set; }
        public string Char { get; internal set; }
        public ICommand Cmd { get; internal set; }
    }
}