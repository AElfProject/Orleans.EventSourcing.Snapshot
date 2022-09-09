using EventStore.ClientAPI;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace SimpleSample.KV;

public class KvConnectionPartitionHelper:IKvConnection
{
    private  string KvConnectionSTR = APIConfHelper.AppSettings["KvConnectionSTR"].ToString();
    private static object KvLock = new object();
    private  ConnectionMultiplexer _connection;
    private static int PartitionSize =Convert.ToInt32(APIConfHelper.AppSettings["PartitionSize"]);
 
 
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
        var result = ReadStreamTest(stream,start,count);

        foreach (var redisValue in result)
        {
            EventData eventData = JsonConvert.DeserializeObject<EventData>(redisValue);
            list.Add(eventData);
        }
        return list;
    }

    public async Task<List<string>> ReadStreamEventsForwardTestAsync(string stream, long start, int count)
    {
        List<string> list = new List<string>();
        var result = ReadStreamTest(stream,start,count);

        foreach (var redisValue in result)
        {
            string eventData = JsonConvert.DeserializeObject<string>(redisValue);
            list.Add(eventData);
        }
        return list;
    }
    
    public List<RedisValue> ReadStreamTest(string stream, long start, int count)
    {
        List<RedisValue> allList = new List<RedisValue>();
        var endRegion = (start + count) / PartitionSize +
            ((start + count) % PartitionSize > 0
                ? 1
                : 0);
        
        var startRegion=start/PartitionSize + (start % PartitionSize>0?1:0 );
        var crossPartitionCount = endRegion - startRegion;
        var keyName =stream+"_"+ GetPartition(start);
        var end = crossPartitionCount > 0 ? PartitionSize - 1 : start + count - 1;
        var result=  GetRedisDatabase().ListRange(keyName, start % PartitionSize-1,end );
        allList.AddRange(result);
        for (int i = 1; i <= crossPartitionCount; i++)
        {
            keyName =stream+"_"+ GetPartition(start+PartitionSize*i);
            RedisValue[] tempRe;
            if (i!=crossPartitionCount)
            {
                tempRe=  GetRedisDatabase().ListRange(keyName, 0, PartitionSize-1);
            }
            else
            {
                tempRe=  GetRedisDatabase().ListRange(keyName, 0, (start % PartitionSize + count) % PartitionSize-1);
            }
            allList.AddRange(tempRe);
        }

        return allList;

    }
    
    

    public async Task<bool> AppendToStreamAsync(string stream, long expectedVersion, IEnumerable<EventData> events
        )
    {
        int eventCount = events.Count();
        var tempversion = expectedVersion+1 - eventCount;
        bool result = true;
        foreach (var eventData in events)
        {
            var keyName =stream+"_"+ GetPartition(tempversion);
            
            var l=await GetRedisDatabase().ListRightPushAsync(keyName,JsonConvert.SerializeObject(eventData));
            if (l<1)
            {
                result = false;
            }

            tempversion += 1;
        }


        return result;
    }

    public async Task<bool> AppendToStreamTestAsync(string stream, long expectedVersion, List<string> events
    )
    {
        int eventCount = events.Count();
        var tempversion = expectedVersion+2 - eventCount;
        bool result = true;
        foreach (var eventData in events)
        {
            var keyName =stream+"_"+ GetPartition(tempversion);
            
            var l= GetRedisDatabase().ListRightPush(keyName,JsonConvert.SerializeObject(eventData));
            if (l<1)
            {
                result = false;
            }

            tempversion += 1;
        }


        return result;
    }
    
    public async Task<EventData> ReadStreamEventsBackwardAsync(string stream, long start, int count)
    {
        var keyName =stream+"_"+ GetPartition(start);
        var result=  GetRedisDatabase().ListRange(keyName, -1,-1);
        EventData eventData = JsonConvert.DeserializeObject<EventData>(result[0]);
        return eventData;
    }

    private long  GetPartition(long version)
    {
        if (version==0)
        {
            return 0;
        }
        var integer = version / PartitionSize;
        var remainder = version % PartitionSize;
        return integer + (remainder>0?1:0)-1;
    }

}