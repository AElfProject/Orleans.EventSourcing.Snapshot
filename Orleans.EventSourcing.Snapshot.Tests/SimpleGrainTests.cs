using Orleans.TestingHost;
using TestGrainInterfaces;
using Xunit;

namespace Orleans.EventSourcing.Snapshot.Tests;

[Collection(ClusterCollection.Name)]
public class SimpleGrainTests
{
    private readonly TestCluster _cluster;

    public SimpleGrainTests(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }
    

    [Fact]
    public async Task CalculateGrain_Add_Test()
    {
        var grain = _cluster.GrainFactory.GetGrain<ICalculateGrain>(3);

        Task<int> result = grain.CalculateAddition(6, 7);
        Assert.Equal(13,await result);
    }

    [Fact]
    public async Task NumberGrain_Push_Test()
    {
        var grain = _cluster.GrainFactory.GetGrain<INumberGrain>(Guid.NewGuid());

        await grain.PushNumber(5);
        await grain.PushNumber(4);
        await grain.PushNumber(11);
        await grain.PushNumber(6);

        Task<int> sum = grain.GetTotalSum();
        Assert.Equal(26,await sum);

        Task<int> snapshotSum = grain.GetSnapshotSum();
        Assert.Equal(9,await snapshotSum);

        Task<int> snapshotTotalSum = grain.GetSnapshotTotalSum();
        Assert.Equal(9,await snapshotTotalSum);

        await grain.PushNumber(2);
        await grain.PushNumber(17);
        await grain.PushNumber(10);
        await grain.PushNumber(4);
        
        Task<int> sum2 = grain.GetTotalSum();
        Assert.Equal(59,await sum2);
        
        Task<int> snapshotSum2 = grain.GetSnapshotSum();
        Assert.Equal(28,await snapshotSum2);
        
        Task<int> snapshotTotalSum2 = grain.GetSnapshotTotalSum();
        Assert.Equal(28,await snapshotTotalSum2);
    }
}