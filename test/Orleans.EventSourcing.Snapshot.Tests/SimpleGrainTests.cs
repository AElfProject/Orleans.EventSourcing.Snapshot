using System;
using System.Threading.Tasks;
using Orleans.TestingHost;
using Shouldly;
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

        int result = await grain.CalculateAddition(6, 7);
        result.ShouldBe(13);
    }

    [Fact]
    public async Task NumberGrain_Push_Test()
    {
        var grain = _cluster.GrainFactory.GetGrain<INumberGrain>(Guid.NewGuid());

        await grain.PushNumber(5);
        await grain.PushNumber(4);
        await grain.PushNumber(11);
        await grain.PushNumber(6);
        
        int sum = await grain.GetTotalSum();
        sum.ShouldBe(26);

        int snapshotSum = await grain.GetSnapshotSum();
        snapshotSum.ShouldBe(20);

        int snapshotTotalSum = await grain.GetSnapshotTotalSum();
        snapshotTotalSum.ShouldBe(20);

        await grain.PushNumber(2);
        await grain.PushNumber(17);
        await grain.PushNumber(10);
        await grain.PushNumber(4);
        
        int sum2 = await grain.GetTotalSum();
        sum2.ShouldBe(59);

        int snapshotSum2 = await grain.GetSnapshotSum();
        snapshotSum2.ShouldBe(45);

        int snapshotTotalSum2 = await grain.GetSnapshotTotalSum();
        snapshotTotalSum2.ShouldBe(45);
    }
}