using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Logger
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public interface ILogger
    {
        bool EnableCmdrLogInfo { get; set; }
        bool EnableCmdrLogTrace { get; set; }

        void logInfo(string messageTemplate);
        void logInfo<T0>(string messageTemplate, T0 p0);
        void logInfo<T0, T1>(string messageTemplate, T0 p0, T1 p1);
        void logInfo<T0, T1, T2>(string messageTemplate, T0 p0, T1 p1, T2 p2);
        void logInfo<T0, T1, T2, T3>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3);
        void logInfo<T0, T1, T2, T3, T4>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4);
        void logInfo<T0, T1, T2, T3, T4, T5>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);

        void logInfo<T0, T1, T2, T3, T4, T5, T6>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5,
            T6 p6);

        void logInfo<T0, T1, T2, T3, T4, T5, T6, T7>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5,
            T6 p6, T7 p7);

        void logInfo<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6, T7 p7, T8 p8);

        void logInfo<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);


        void logDebug(string messageTemplate);
        void logDebug<T0>(string messageTemplate, T0 p0);
        void logDebug<T0, T1>(string messageTemplate, T0 p0, T1 p1);
        void logDebug<T0, T1, T2>(string messageTemplate, T0 p0, T1 p1, T2 p2);
        void logDebug<T0, T1, T2, T3>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3);
        void logDebug<T0, T1, T2, T3, T4>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4);
        void logDebug<T0, T1, T2, T3, T4, T5>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);

        void logDebug<T0, T1, T2, T3, T4, T5, T6>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5,
            T6 p6);

        void logDebug<T0, T1, T2, T3, T4, T5, T6, T7>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5,
            T6 p6, T7 p7);

        void logDebug<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6, T7 p7, T8 p8);

        void logDebug<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);


        void logWarning(System.Exception exception, string messageTemplate);
        void logWarning<T0>(System.Exception exception, string messageTemplate, T0 p0);
        void logWarning<T0, T1>(System.Exception exception, string messageTemplate, T0 p0, T1 p1);
        void logWarning<T0, T1, T2>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2);
        void logWarning<T0, T1, T2, T3>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3);

        void logWarning<T0, T1, T2, T3, T4>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2,
            T3 p3, T4 p4);

        void logWarning<T0, T1, T2, T3, T4, T5>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2,
            T3 p3, T4 p4, T5 p5);

        void logWarning<T0, T1, T2, T3, T4, T5, T6>(System.Exception exception, string messageTemplate, T0 p0, T1 p1,
            T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);

        void logWarning<T0, T1, T2, T3, T4, T5, T6, T7>(System.Exception exception, string messageTemplate, T0 p0,
            T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);

        void logWarning<T0, T1, T2, T3, T4, T5, T6, T7, T8>(System.Exception exception, string messageTemplate, T0 p0,
            T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);

        void logWarning<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(System.Exception exception, string messageTemplate,
            T0 p0, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);


        void logError(System.Exception exception, string messageTemplate);
        void logError<T0>(System.Exception exception, string messageTemplate, T0 p0);
        void logError<T0, T1>(System.Exception exception, string messageTemplate, T0 p0, T1 p1);
        void logError<T0, T1, T2>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2);
        void logError<T0, T1, T2, T3>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3);

        void logError<T0, T1, T2, T3, T4>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2,
            T3 p3, T4 p4);

        void logError<T0, T1, T2, T3, T4, T5>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2,
            T3 p3, T4 p4, T5 p5);

        void logError<T0, T1, T2, T3, T4, T5, T6>(System.Exception exception, string messageTemplate, T0 p0, T1 p1,
            T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);

        void logError<T0, T1, T2, T3, T4, T5, T6, T7>(System.Exception exception, string messageTemplate, T0 p0, T1 p1,
            T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);

        void logError<T0, T1, T2, T3, T4, T5, T6, T7, T8>(System.Exception exception, string messageTemplate, T0 p0,
            T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);

        void logError<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(System.Exception exception, string messageTemplate, T0 p0,
            T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);
    }
}