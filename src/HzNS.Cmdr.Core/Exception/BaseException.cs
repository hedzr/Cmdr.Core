#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using HzNS.Cmdr.Base;

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
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class WarnFlagException : CmdrException
    {
        public WarnFlagException(bool isShort, string ch, IFlag flag, ICommand owner)
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

    public class DuplicationFlagCharException : WarnFlagException
    {
        public DuplicationFlagCharException(bool isShort, string ch, IFlag flag, ICommand owner) : base(isShort, ch,
            flag, owner)
        {
        }
    }

    public class EmptyFlagLongFieldException : WarnFlagException
    {
        public EmptyFlagLongFieldException(bool isShort, string ch, IFlag flag, ICommand owner) : base(isShort, ch,
            flag, owner)
        {
        }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class WarnCommandException : CmdrException
    {
        public WarnCommandException(bool isShort, string ch, ICommand cmd)
        {
            IsShort = isShort;
            Char = ch;
            Cmd = cmd;
        }

        public bool IsShort { get; internal set; }
        public string Char { get; internal set; }
        public ICommand Cmd { get; internal set; }
    }

    public class DuplicationCommandCharException : WarnCommandException
    {
        public DuplicationCommandCharException(bool isShort, string ch, ICommand cmd) : base(isShort, ch, cmd)
        {
        }
    }

    public class EmptyCommandLongFieldException : WarnCommandException
    {
        public EmptyCommandLongFieldException(bool isShort, string ch, ICommand cmd) : base(isShort, ch, cmd)
        {
        }
    }
}