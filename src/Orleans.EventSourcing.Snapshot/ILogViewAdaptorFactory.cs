using Orleans.LogConsistency;
using Orleans.Storage;

namespace Orleans.EventSourcing.Snapshot;

/// <summary>
/// Interface to be implemented for a log-view adaptor factory
/// </summary>
public interface ILogViewAdaptorFactory  
{
    /// <summary> Returns true if a storage provider is required for constructing adaptors. </summary>
    bool UsesStorageProvider { get; }

    /// <summary>
    /// Construct a <see cref="LogConsistency.ILogViewAdaptor{TLogView,TLogEntry}"/> to be installed in the given host grain.
    /// </summary>
    Snapshot.ILogViewAdaptor<TLogView, TLogEntry> MakeLogViewAdaptor<TLogView, TLogEntry>(
        ILogViewAdaptorHost<TLogView, TLogEntry> hostgrain,
        TLogView initialstate,
        string graintypename,
        IGrainStorage grainStorage,
        ILogConsistencyProtocolServices services)

        where TLogView : class, new()
        where TLogEntry : class;

}