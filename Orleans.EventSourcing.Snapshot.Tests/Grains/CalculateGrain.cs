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
        
        public void Apply(EventSub @event)
        {
            resultList.Add(@event.num1-@event.num2);
        }

        public void Apply(EventMul @event)
        {
            resultList.Add(@event.num1*@event.num2);
        }
        
        public void Apply(EventDiv @event)
        {
            resultList.Add(@event.num1/@event.num2);
        }
    }
    
    public async Task<int> CalculateAddition(int a, int b)
    {
        RaiseEvent(new EventAdd { num1 = a, num2 = b }, true);
        await ConfirmEvents();
        return (a + b);
    }

    public async Task<int> CalculateSubtraction(int x, int y)
    {
        RaiseEvent(new EventMul { num1 = x, num2 = y }, true);
        await ConfirmEvents();
        return (x - y);
    }

    public async Task<int> CalculateMultiplication(int i, int j)
    {
        RaiseEvent(new EventMul { num1 = i, num2 = j }, true);
        await ConfirmEvents();
        return (i * j);
    }

    public async Task<int> CalculateDivision(int o, int p)
    {
        RaiseEvent(new EventMul { num1 = o, num2 = p }, true);
        await ConfirmEvents();
        return (o / p);
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
public class EventSub
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

[Serializable]
public class EventDiv
{
    public int num1 { get; set; }
    public int num2 { get; set; }
}