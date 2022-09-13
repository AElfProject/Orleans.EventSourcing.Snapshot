using EventStore.ClientAPI;
using JsonNet.PrivateSettersContractResolvers;
using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing.Snapshot.Hosting;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.TestingHost;
using TestGrainInterfaces;
using TestGrains;

namespace Orleans.EventSourcing.Snapshot.Tests;

public class SnapshotClusterFixture: IDisposable
{
    public SnapshotClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        this.Cluster = builder.Build();
        this.Cluster.Deploy();
    }
    
    public TestCluster Cluster { get; private set; }
    
    public void Dispose()
    {
        this.Cluster.StopAllSilos();
    }

    private class TestSiloConfigurations : ISiloBuilderConfigurator {
        public void Configure(ISiloHostBuilder hostBuilder) {
            hostBuilder.ConfigureServices(services => { 
                    services.AddSingleton<ICalculateGrain, CalculateGrain>();
                services.AddSingleton<INumberGrain, NumberGrain>();
            }).UseMongoDBClient("mongodb://localhost:27017")
                .AddMongoDBGrainStorageAsDefault((MongoDBGrainStorageOptions op) =>
                {
                    op.CollectionPrefix = "GrainStorage";
                    op.DatabaseName = "SimpleSampleOrelans";

                    op.ConfigureJsonSerializerSettings = jsonSettings =>
                    {
                        jsonSettings.ContractResolver = new PrivateSetterContractResolver();
                    };
                })
                .AddSnapshotStorageBasedLogConsistencyProviderAsDefault((op, name) => 
                {
                    // Take snapshot every five events
                    op.SnapshotStrategy = strategyInfo => strategyInfo.CurrentConfirmedVersion - strategyInfo.SnapshotVersion >= 5;
                    op.UseIndependentEventStorage = true;
                    // Should configure event storage when set UseIndependentEventStorage true
                    op.ConfigureIndependentEventStorage = (services, name) =>
                    {
                        var eventStoreConnectionString = "ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500";
                        var eventStoreConnection = EventStoreConnection.Create(eventStoreConnectionString);
                        eventStoreConnection.ConnectAsync().Wait();

                        services.AddSingleton(eventStoreConnection);
                        services.AddSingleton<IGrainEventStorage, EventStoreGrainEventStorage>();
                    };
                });
        }
    }
}