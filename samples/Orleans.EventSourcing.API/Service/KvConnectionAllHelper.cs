using System.Diagnostics;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Orleans.EventSourcing.API.Helpers;
using StackExchange.Redis;

namespace Orleans.EventSourcing.API.Service;

public class KvConnectionAllHelper :IKvConnection
{
    private const string WriteScript = " local  stream= KEYS[1]; " +
                                       "local  events=ARGV;" +
                                       "local  result=true;" +
                                       "local  version=0;" +
                                       "local tempversion = redis.call('get',string.format('%s_version',stream )); " +
                                       "if (tempversion==false) then " +
                                       " version=0 ;" +
                                       " else   version=tempversion; " +
                                       " end " +

                                       " for i ,event in ipairs(events) do " +
                                       " version=version+1; " +
                                       " result =redis.call('set',string.format('%s_%s',stream,version ) ,event); end " +
                                       " redis.call('set',string.format('%s_version',stream ) ,version)  return result ";
    private const int ReloadWriteScriptMaxCount = 3;
    private static object KvLock = new object();
    private  ConnectionMultiplexer _connection;
    private LuaScript _preparedWriteScript;
    private byte[] _preparedWriteScriptHash;

    /*public KvConnectionAllHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }*/
    public IConfiguration _configuration { get; }

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
       
        return   Instance.GetDatabase();
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

        for (int i = 0; i < count; i++)
        {
            var redisValue = await GetRedisDatabase().StringGetAsync(stream + (start + i));
            list.Add(JsonConvert.DeserializeObject<EventData>(redisValue));
        }

        return list;
    }

    public async Task<bool> AppendToStreamAsync(string stream, long expectedVersion, IEnumerable<object> events)
    {
        var redisR = WriteToRedisUsingPreparedScriptAsync(stream,events);
        
        return redisR!=null;
    }

    private Task<RedisResult> WriteToRedisUsingPreparedScriptAsync(string stream,  IEnumerable<object> events)
    {
        var keys = new RedisKey[] { stream };
        //var args =  Array.ConvertAll(values.ToArray(), item => (RedisValue)item);
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
        List<EventData> list = new List<EventData>();

        for (int i = 0; i < count; i++)
        {
            var redisValue = await GetRedisDatabase().StringGetAsync(stream + (start - i));
            list.Add(JsonConvert.DeserializeObject<EventData>(redisValue));
        }

        return list;
    }
}