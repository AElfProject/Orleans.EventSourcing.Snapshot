using StackExchange.Redis;

namespace Orleans.EventSourcing.API.Helpers;

public class RedisHelper
{
    //private static readonly ConfigurationOptions ConfigurationOptions = RedisHelper.ConfigurationOptions.Parse("127.0.0.1:6379,password=123456");
    private static readonly object Locker = new object();
    private static ConnectionMultiplexer _redisConn;

    /// <summary>
    /// 单例获取
    /// </summary>
    public static ConnectionMultiplexer RedisConn
    {
        get
        {
            if (_redisConn == null)
            {
                // 锁定某一代码块，让同一时间只有一个线程访问该代码块
                lock (Locker)
                {
                    if (_redisConn == null || !_redisConn.IsConnected)
                    {
                        _redisConn = ConnectionMultiplexer.Connect("");
                    }
                }
            }
            return _redisConn;
        }
    }

}