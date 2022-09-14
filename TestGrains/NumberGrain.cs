using Orleans.EventSourcing.Snapshot;
using TestGrainInterfaces;

namespace TestGrains;

public class NumberGrain:JournaledSnapshotGrain<NumberGrain.GrainState>,INumberGrain
{
    public class GrainState
    {
        public List<int> numList { get; set; } = new List<int>();
        
        public int totalSum { get; private set; }

        public void Apply(EventPush @event)
        {
            numList.Add(@event.num);
            totalSum = totalSum + @event.num;
        }
    }
    
    public async Task PushNumber(int a)
    {
        bool isNeedStorageSnapshot = false;
        if (a > 10)
        {
            isNeedStorageSnapshot = true;
            // this.State.numList=
        }
        RaiseEvent(new EventPush { num = a}, isNeedStorageSnapshot);
        await ConfirmEvents();
        return;
    }

    public Task<int> GetTotalSum()
    {
        return Task.FromResult(this.State.totalSum);
    }

    public async Task<int> GetSnapshotSum()
    {
        SnapshotStateWithMetaData<GrainState, object> metaData = await GetLastSnapshotMetaData();

        List<int> snapshotList = metaData.Snapshot.numList;

        int sum = 0;
        foreach (int num in snapshotList)
        {
            sum = sum + num;
        }

        return sum;
    }

    public async Task<int> GetSnapshotTotalSum()
    {
        SnapshotStateWithMetaData<GrainState, object> metaData = await GetLastSnapshotMetaData();
        
        int snapshotTotalSum = metaData.Snapshot.totalSum;

        return snapshotTotalSum;
    }
}

[Serializable]
public class EventPush
{
    public int num { get; set; }
}