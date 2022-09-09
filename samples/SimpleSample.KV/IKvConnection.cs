using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace SimpleSample.KV;

public interface IKvConnection
{
    Task<List<EventData>> ReadStreamEventsForwardAsync(
        string stream,
        long start,
        int count);
    
    Task<bool> AppendToStreamAsync(
        string stream,
        long expectedVersion,
        IEnumerable<EventData> events);

    Task<EventData> ReadStreamEventsBackwardAsync(
        string stream,
        long start,
        int count);
}