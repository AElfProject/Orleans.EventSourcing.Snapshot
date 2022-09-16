using Orleans.EventSourcing.Snapshot;
using Orleans.Providers;

namespace Orleans.EventSourcing.Snapshot.Tests;

// [StorageProvider(ProviderName = "Default")]
// [LogConsistencyProvider(ProviderName = "LogStorage")]
public class CalculateGrain:JournaledSnapshotGrain<CalculateGrain.GrainState>,ICalculateGrain
{
    [Serializable]
    public class GrainState
    {
        public List<int> resultList { get; private set; } = new List<int>();

        public void Apply(EventAdd @event)
        {
            resultList.Add(@event.num1+@event.num2);
        }

        public void Apply(EventMul @event)
        {
            resultList.Add(@event.num1*@event.num2);
        }
    }
    
    public Task<int> CalculateAddition(int a, int b)
    {
        RaiseEvent(new EventAdd { num1 = a, num2 = b }, true);
        ConfirmEvents();
        return Task.FromResult(a + b);
    }

    public Task<int> CalculateSubtraction(int x, int y)
    {
        return Task.FromResult(x - y);
    }

    public Task<int> CalculateMultiplication(int i, int j)
    {
        RaiseEvent(new EventMul { num1 = i, num2 = j }, true);
        ConfirmEvents();
        return Task.FromResult(i * j);
    }

    public Task<int> CalculateDivision(int o, int p)
    {
        return Task.FromResult(o / p);
    }

    public Task<List<int>> GetResultList()
    {
        return Task.FromResult(this.State.resultList);
    }
}


[Serializable]
public class EventAdd
{
    public int num1 { get; set; }
    public int num2 { get; set; }
}

[Serializable]
public class EventMul
{
    public int num1 { get; set; }
    public int num2 { get; set; }
}