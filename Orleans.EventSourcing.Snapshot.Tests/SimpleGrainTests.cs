using Orleans.TestingHost;
using TestGrainInterfaces;
using Xunit;

namespace Orleans.EventSourcing.Snapshot.Tests;

[Collection(SnapshotClusterCollection.Name)]
public class SimpleGrainTests
{
    private readonly TestCluster _cluster;

    public SimpleGrainTests(SnapshotClusterFixture fixture)
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
        await grain.PushNumber(3);
        await grain.PushNumber(12);
        await grain.PushNumber(6);

        Task<int> sum = grain.GetTotalSum();
        Assert.Equal(26,await sum);
    }
}