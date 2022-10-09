using Xunit;

namespace Orleans.EventSourcing.Snapshot.Tests;

[CollectionDefinition(SnapshotClusterCollection.Name)]
public class SnapshotClusterCollection: ICollectionFixture<SnapshotClusterFixture>
{
    public const string Name = "SnapshotClusterCollection";
}