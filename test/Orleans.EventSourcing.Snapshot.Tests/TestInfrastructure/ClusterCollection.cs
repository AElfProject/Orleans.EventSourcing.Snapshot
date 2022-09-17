using Xunit;

namespace Orleans.EventSourcing.Snapshot.Tests;

[CollectionDefinition(ClusterCollection.Name)]
public class ClusterCollection: ICollectionFixture<ClusterFixture>
{
    public const string Name = "ClusterCollection";
}