using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace SimpleSample.KV;

public class KvConnectionAllHelper :IKvConnection
{
    private  string KvConnectionSTR = APIConfHelper.AppSettings["KvConnectionSTR"].ToString();
    private static object KvLock = new object();
    private  ConnectionMultiplexer _connection;

    public   ConnectionMultiplexer Instance
    {
        get
        {
            if (_connection == null)
            {
                lock (KvLock)
                {
                    if (_connection == null || _connection.IsConnected == false)
                    {
                        _connection = ConnectionMultiplexer.Connect(KvConnectionSTR);
                    }
                }
            }
 
 
            return _connection;
        }  //end get
    }
 
 
    public  IDatabase GetRedisDatabase()
    {
        return Instance.GetDatabase();
    }


    
    
    public async Task<List<EventData>> ReadStreamEventsForwardAsync(string stream, long start, int count)
    {
        List<EventData> list = new List<EventData>();

        for (int i = 0; i < count-1; i++)
        {
            var redisValue = await GetRedisDatabase().StringGetAsync(stream + (start + i));
            list.Add(JsonConvert.DeserializeObject<EventData>(redisValue));
        }

        return list;
    }

    public async Task<bool> AppendToStreamAsync(string stream, long expectedVersion, IEnumerable<EventData> events)
    {
        int eventCount = events.Count();
        var tempversion = expectedVersion - eventCount+1;
        bool result = true;
        foreach (var eventData in events)
        { 
            var value=GetRedisDatabase().StringSet(stream+tempversion.ToString(), JsonConvert.SerializeObject(eventData));
            tempversion += 1;
            if (!value)
            {
                result = false;
            }
        }
        return result;
    }

    public async Task<EventData> ReadStreamEventsBackwardAsync(string stream, long start, int count)
    {
        var redisValue = await GetRedisDatabase().StringGetAsync(stream + start );
        return JsonConvert.DeserializeObject<EventData>(redisValue);
    }
}