using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace SimpleSample.KV;

public class KvConnectionListHelper:IKvConnection
{
    //private  string KvConnectionSTR = APIConfHelper.AppSettings["KvConnectionSTR"].ToString();
    private static object KvLock = new object();
    private static ConnectionMultiplexer _connection;
 
 
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
                        var KvConnectionSTR = APIConfHelper.AppSettings["KvConnectionSTR"].ToString();
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

        var result=  GetRedisDatabase().ListRange(stream, start, count+start-1);
        //var result2=  GetRedisDatabase().ListRange(stream, start, -1);
        List<EventData> list = new List<EventData>();
        foreach (var redisValue in result)
        {
            EventData eventData = JsonConvert.DeserializeObject<EventData>(redisValue);
            list.Add(eventData);
        }

        return list;
      
    }
    
    public async Task<bool> AppendToStreamAsync(string stream, long expectedVersion, IEnumerable<EventData> events)
    {
        
        foreach (var eventData in events)
        {
            await GetRedisDatabase().ListRightPushAsync(stream,JsonConvert.SerializeObject(eventData));
        }

        return true;
    }

    public async Task<EventData> ReadStreamEventsBackwardAsync(string stream, long start, int count)
    {
        var result=  await GetRedisDatabase().ListRangeAsync(stream, -1, -1);

        return result.Any()?JsonConvert.DeserializeObject<EventData>(result.First()): default ;
    }

    public async Task<Demotest> ReadStreamEventsBackwardTestAsync(string stream, long start, int count)
    {
        var result=   GetRedisDatabase().ListRange(stream, -1, -1);

        return result.Any()?JsonConvert.DeserializeObject<Demotest>(result.First()): default ;
    }
    
 
    public class Demotest
    {
        public string name { set; get; }
        public string age { set; get; }
    }
    


}