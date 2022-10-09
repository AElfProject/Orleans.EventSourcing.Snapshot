﻿using EventStore.ClientAPI;
using JsonNet.PrivateSettersContractResolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using AElf.Orleans.EventSourcing.Snapshot;
using AElf.Orleans.EventSourcing.Snapshot.Hosting;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using SimpleSample.Grains;
using System;
using System.Threading.Tasks;

namespace SimpleSample.Silo
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var host = BuildSilo();

            await host.StartAsync();
            Console.WriteLine("SimpleSample silo started");

            Console.ReadLine();
        }

        private static ISiloHost BuildSilo()
        {
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(PersonGrain).Assembly).WithReferences())
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug).AddConsole();
                })
                .UseMongoDBClient("mongodb://localhost:27017")
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

            return builder.Build();
        }
    }
}
