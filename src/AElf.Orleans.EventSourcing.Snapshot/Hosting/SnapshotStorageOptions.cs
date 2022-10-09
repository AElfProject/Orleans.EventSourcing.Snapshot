using Microsoft.Extensions.DependencyInjection;
using System;

namespace AElf.Orleans.EventSourcing.Snapshot.Hosting
{
    public class SnapshotStorageOptions
    {
        public bool UseIndependentEventStorage { get; set; } = false;
        
        public Action<IServiceCollection, string> ConfigureIndependentEventStorage { get; set; } = null;
    }
}
