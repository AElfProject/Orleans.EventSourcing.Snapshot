using System.Threading.Tasks;

namespace Orleans.EventSourcing.Snapshot;

public interface ILogViewSnapshotAdaptor<TView, TLogEntry>:ILogViewAdaptor<TView,TLogEntry>
    where TView : new()
{
    /// <summary>
    /// Interface for setting a flag to determine whether to write a state snapshot 
    /// </summary>
    void SetNeedSnapshotFlag();
    
    /// <summary>
    /// get the latest storage of snapshot meta data
    /// </summary>
    /// <returns>snapshot's meta data</returns>
    Task<SnapshotStateWithMetaData<TView, TLogEntry>> GetLastSnapshotMetaDataAsync();
}