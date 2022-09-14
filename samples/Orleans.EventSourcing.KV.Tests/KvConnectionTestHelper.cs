using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Orleans.EventSourcing.KV.Tests;

public class KvConnectionTestHelper
{
    public async Task<List<string>> ReadStreamEventsForwardTestAsync(string stream, long start, int count)
    {
        KvConnectionPartitionHelper helper = new KvConnectionPartitionHelper();
        List<string> list = new List<string>();
        var result = helper.ReadStreamData(stream,start,count);

        foreach (var redisValue in result)
        {
            string eventData = JsonConvert.DeserializeObject<string>(redisValue);
            list.Add(eventData);
        }
        return list;
    }
  

}