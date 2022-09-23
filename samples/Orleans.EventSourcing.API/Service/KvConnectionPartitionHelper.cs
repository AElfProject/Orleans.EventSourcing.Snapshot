using System.Diagnostics;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Orleans.EventSourcing.API.Helpers;
using StackExchange.Redis;

namespace Orleans.EventSourcing.API.Service;

public class KvConnectionPartitionHelper:IKvConnection
{
    private static object KvLock = new object();
    private  ConnectionMultiplexer _connection;
    private readonly int PartitionSize =int.Parse(AppConfigurtaionServices.Configuration
        .GetSection("RedisSetting:PartitionSize").Value);

    private const string WriteScript = "local  stream= KEYS[1]; " +
                                       "local  partitionSize = KEYS[2];" +
                                       "local  events=ARGV; " +
                                       "local  result=true; " +
                                       "local  version=0; " +
                                       "local tempversion = redis.call('get',string.format('%s_version',stream )); " +
                                       "if (tempversion==false) then " +
                                       "version=0 ; " +
                                       "else   version=tempversion; end " +
                                       "for i ,event in ipairs(events) do " +
                                       "version=version+1; " +
                                       "local keyname=''; " +
                                       "local integer =  version / partitionSize ; " +
                                       "local integerS=integer..''; " +
                                       "local index=string.find(integerS, '.', 1) ;" +
                                       "integerS=string.sub(integerS,1,index);" +
                                       "local remainder = version % partitionSize ; " +
                                       " if (remainder>0) then " +
                                       " remainder=1; " +
                                       " else " +
                                       "remainder=0; end " +
                                       "keyname= integerS + remainder -1; " +
                                       "result =redis.call('RPUSH',string.format('%s_%s',stream, keyname) ,event); end " +
                                       "redis.call('set',string.format('%s_version',stream ) ,version) " +
                                       "return result ";
    private const int ReloadWriteScriptMaxCount = 3;
    private LuaScript _preparedWriteScript;
    private byte[] _preparedWriteScriptHash;
    
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
            _preparedWriteScript = LuaScript.Prepare(WriteScript);
            _preparedWriteScriptHash =  LoadWriteScriptAsync();
 
            return _connection;
        }  //end get
    }
 
 
    public  IDatabase GetRedisDatabase()
    {
        return Instance.GetDatabase();
    }
    private byte[] LoadWriteScriptAsync()
    {
        Debug.Assert(_connection is not null);
        Debug.Assert(_preparedWriteScript is not null);

        System.Net.EndPoint[] endPoints = _connection.GetEndPoints();
        var loadTasks = new Task<LoadedLuaScript>[endPoints.Length];
        for (int i = 0; i < endPoints.Length; i++)
        {
            var endpoint = endPoints.ElementAt(i);
            var server = _connection.GetServer(endpoint);

            loadTasks[i] = _preparedWriteScript.LoadAsync(server);
        }
        Task.WhenAll(loadTasks);
        return loadTasks[0].Result.Hash;
    }


    public async Task<List<EventData>> ReadStreamEventsForwardAsync(string stream, long start, int count)
    {
        List<EventData> list = new List<EventData>();
        var result = ReadStreamData(stream,start,count);

        foreach (var redisValue in result)
        {
            EventData eventData = JsonConvert.DeserializeObject<EventData>(redisValue);
            list.Add(eventData);
        }
        return list;
    }

    
    
    public List<RedisValue> ReadStreamData(string stream, long start, int count)
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
    
    

    public async Task<bool> AppendToStreamAsync(string stream, long expectedVersion, IEnumerable<object> events)
    {
        bool result = true;
        var redisR = WriteToRedisUsingPreparedScriptAsync(stream,events);
        /*foreach (var eventData in events)
        {
            var keyName =stream+"_"+ GetPartition(tempversion);
            
            var l=await GetRedisDatabase().ListRightPushAsync(keyName,JsonConvert.SerializeObject(eventData));
            if (l<1)
            {
                result = false;
            }

            tempversion += 1;
        }*/
        return result;
    }

    private Task<RedisResult> WriteToRedisUsingPreparedScriptAsync(string stream,  IEnumerable<object> events)
    {
        var keys = new RedisKey[] { stream, PartitionSize.ToString()};
        var args = events.Select(e => new RedisValue(JsonConvert.SerializeObject(e))).ToArray();
        return WriteToRedisUsingPreparedScriptAsync(attemptNum: 0);
    
        async Task<RedisResult> WriteToRedisUsingPreparedScriptAsync(int attemptNum)
        {
            try
            {
                return await GetRedisDatabase().ScriptEvaluateAsync(_preparedWriteScriptHash, keys, args).ConfigureAwait(false);
            }
            catch (RedisServerException rse) when (rse.Message is not null && rse.Message.StartsWith("NOSCRIPT ", StringComparison.Ordinal))
            {
                // EVALSHA returned error 'NOSCRIPT No matching script. Please use EVAL.'.
                // This means that SHA1 cache of Lua scripts is cleared at server side, possibly because of Redis server rebooted after Init() method was called. Need to reload Lua script.
                // Several attempts are made just in case (e.g. if Redis server is rebooted right after previous script reload).
                if (attemptNum >= ReloadWriteScriptMaxCount)
                {
                    throw;
                }

                LoadWriteScriptAsync();
                return await WriteToRedisUsingPreparedScriptAsync(attemptNum: attemptNum + 1)
                    .ConfigureAwait(false);
            }
        }
       
        
       
    }
    public async Task<List<EventData>> ReadStreamEventsBackwardAsync(string stream, long start, int count)
    {
        var keyName =stream+"_"+ GetPartition(start);
        var result= await GetRedisDatabase().ListRangeAsync(keyName, -1,-1);
        List<EventData> list = new List<EventData>();
        foreach (var redisValue in result)
        {
            EventData eventData = JsonConvert.DeserializeObject<EventData>(redisValue);
            list.Add(eventData);
        }
        return list;
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