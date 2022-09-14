using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing.Snapshot.Hosting;
using Orleans.Hosting;
using Orleans.TestingHost;
using TestGrainInterfaces;
using TestGrains;

namespace Orleans.EventSourcing.Snapshot.Tests;

public class ClusterFixture: IDisposable
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
    }

    public TestCluster Cluster { get; private set; }
    
    private class TestSiloConfigurations : ISiloBuilderConfigurator {
        public void Configure(ISiloHostBuilder hostBuilder) {
            hostBuilder.ConfigureServices(services => {
                    services.AddSingleton<ICalculateGrain, CalculateGrain>();
                    services.AddSingleton<INumberGrain, NumberGrain>();
                }).AddMemoryGrainStorageAsDefault()
                .AddSnapshotStorageBasedLogConsistencyProviderAsDefault((op, name) => 
                {
                    // Take snapshot every five events
                    op.SnapshotStrategy = strategyInfo => strategyInfo.CurrentConfirmedVersion - strategyInfo.SnapshotVersion >= 5;
                    op.UseIndependentEventStorage = false;
                });
        }
    }
}


