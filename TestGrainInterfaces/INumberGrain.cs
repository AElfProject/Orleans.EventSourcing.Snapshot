using Orleans;

namespace TestGrainInterfaces;

public interface INumberGrain:IGrainWithGuidKey
{
    Task PushNumber(int num);

    Task<int> GetTotalSum();

    Task<int> GetSnapshotSum();

    Task<int> GetSnapshotTotalSum();
}