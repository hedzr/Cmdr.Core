namespace HzNS.Cmdr.Base
{
    public interface IAppInfo
    {
        string AppName { get; }
        string AppVersion { get; }
        int AppVersionInt { get; }
        
        string Author { get; }
        string Copyright { get; }
        
        string BuildTimestamp { get; }

        /// <summary>
        /// The Tracing Release Tag in Team
        /// </summary>
        string BuildRelease { get; }

        /// <summary>
        /// Git Tag/Version/Branch Name
        /// </summary>
        string BuildVcsVersion { get; }

        /// <summary>
        /// Git Hash
        /// </summary>
        string BuildVcsHash { get; }

        /// <summary>
        /// The building tags.
        /// such as: Debug/Release, ...
        /// </summary>
        string BuildTags { get; }

        /// <summary>
        /// The building command-line arguments from CI tool.
        /// 
        /// such as: --ldflags="-s -w"
        /// </summary>
        string BuildArgs { get; }

        /// <summary>
        /// The Builder Title.
        /// 
        /// such as: gcc v4.8.1
        /// </summary>
        string Builder { get; }
    }
}