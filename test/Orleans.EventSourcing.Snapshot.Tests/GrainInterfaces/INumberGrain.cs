using System.Threading.Tasks;
using Orleans;

namespace Orleans.EventSourcing.Snapshot.Tests;

public interface INumberGrain:IGrainWithGuidKey
{
    Task PushNumber(int num);

    Task<int> GetTotalSum();

    Task<int> GetSnapshotSum();

    Task<int> GetSnapshotTotalSum();
}