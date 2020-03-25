using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace Tests.HzNS
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "PublicConstructorInAbstractClass")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract class TestBase
    {
        // https://xunit.github.io/docs/capturing-output.html

        #region XUnit: capture the output

        public readonly ITestOutputHelper Output;

        public TestBase(ITestOutputHelper output)
        {
            Output = output;
        }

        #endregion
    }
}