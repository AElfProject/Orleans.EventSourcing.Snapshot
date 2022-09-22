using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Orleans.EventSourcing.API.Service;

public interface IKvConnection
{
    public  Task<List<EventData>> ReadStreamEventsForwardAsync(
        string stream,
        long start,
        int count);
    
    public  Task<bool> AppendToStreamAsync(
        string stream,
        long expectedVersion,
        IEnumerable<object> events);

    public  Task<List<EventData>> ReadStreamEventsBackwardAsync(
        string stream,
        long start,
        int count);
}