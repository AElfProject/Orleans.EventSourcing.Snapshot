﻿using Orleans;
using Orleans.Configuration;
using SimpleSample.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SimpleSample.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var clusterClient = await BuildOrleansClient();

            //var personId = Guid.Empty;
            var personId = Guid.ParseExact("669911FF-8888-D000-B44D-00C04FB964FF","D");
            var person = clusterClient.GetGrain<IPersonGrain>(personId);

            Console.WriteLine("Please input your nickname: ");
            var nickName = Console.ReadLine();

            await person.UpdateNickName(nickName);

            while (true)
            {
                Console.WriteLine("Type in what you want to say: ");
                var input = Console.ReadLine();

                await person.Say(input);

                var historySaids = await person.GetHistorySaids();

                Console.WriteLine("Your history saids: ");
                Console.WriteLine(string.Join(Environment.NewLine, historySaids));
                Console.WriteLine("--------------------");

                var snapshotList = await person.GetLastSnapshotSaids();
                Console.WriteLine("Snapshot List:");
                Console.WriteLine(string.Join(Environment.NewLine, snapshotList));
                int globalVersion = await person.GetLastSnapshotGlobalVersion();
                Console.WriteLine("Global Version:" + globalVersion);
                Console.WriteLine();
            }
        }

        private static async Task<IClusterClient> BuildOrleansClient()
        {
            var endPoints = new List<IPEndPoint>();

            var ipAddress = IPAddress.Loopback;
            var port = 30000;

            endPoints.Add(new IPEndPoint(ipAddress, port));

            var client = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "SimpleSample";
                    options.ServiceId = "SimpleSample";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPersonGrain).Assembly).WithReferences())
                .UseStaticClustering(endPoints.ToArray())
                .Build();

            await client.Connect();

            return client;
        }
    }
}
