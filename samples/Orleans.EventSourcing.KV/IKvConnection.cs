using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Orleans.EventSourcing.KV;

public interface IKvConnection
{
    Task<List<EventData>> ReadStreamEventsForwardAsync(
        string stream,
        long start,
        int count);
    
    Task<bool> AppendToStreamAsync(
        string stream,
        long expectedVersion,
        IEnumerable<object> events);

    Task<List<EventData>> ReadStreamEventsBackwardAsync(
        string stream,
        long start,
        int count);
}