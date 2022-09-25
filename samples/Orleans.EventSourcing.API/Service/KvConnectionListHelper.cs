using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Newtonsoft.Json;
using Orleans.EventSourcing.API.Helpers;
using StackExchange.Redis;
using EventData = EventStore.ClientAPI.EventData;

namespace Orleans.EventSourcing.API.Service;

public class KvConnectionListHelper:IKvConnection
{
    //private  string KvConnectionSTR = APIConfHelper.AppSettings["KvConnectionSTR"].ToString();
    //private string KvConnectionSTR = "localhost:6379";
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
                        var KvConnectionSTR = AppConfigurtaionServices.Configuration
                            .GetSection("RedisSetting:KvConnectionSTR").Value;
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

        var result= await GetRedisDatabase().ListRangeAsync(stream, start, count+start-1);
        List<EventData> list = new List<EventData>();
       
        return list;
      
    }

   

    public async Task<bool> AppendToStreamAsync(string stream, long expectedVersion, IEnumerable<object> events)
    {
        var count= await GetRedisDatabase().ListRightPushAsync(stream,
            events.Select(e => new RedisValue(JsonConvert.SerializeObject(e))).ToArray());

        return count > 0 ;
    }



    public async Task<List<EventData>> ReadStreamEventsBackwardAsync(string stream, long start, int count)
    {
        var result=  await GetRedisDatabase().ListRangeAsync(stream, -1, -1);

        
        List<EventData> list = new List<EventData>();
        foreach (var redisValue in result)
        {
            EventData eventData = JsonConvert.DeserializeObject<EventData>(redisValue);
            list.Add(eventData);
        }
        return list;
    }

  
 
    /*
    public class Demotest
    {
        public string name { set; get; }
        public string age { set; get; }
    }

    public void MainTest()
    {
        AddChild(true,1);

    }

    private Dictionary<string, object> AddChild(bool hasChild,int type)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        if (type==1)
        {
           
            Object o = null;
            if (hasChild)
            {
                o = AddChild(true, 2);
            }
            d.Add("子集1",o);
        }
        if (type==2)
        {
            
            if (hasChild)
            {
            
            }
        }

        return d;
    }*/
}