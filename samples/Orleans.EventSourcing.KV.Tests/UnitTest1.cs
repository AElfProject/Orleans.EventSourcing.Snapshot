using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Xunit;

namespace Orleans.EventSourcing.KV.Tests;

public class Tests
{
    [Fact]
    public async Task Test1()
    {
        KvConnectionTestHelper _connection = new KvConnectionTestHelper();
        List<string> list = new List<string> ();
        
        for (int i = 1; i <= 100; i++)
        {
            list.Add(i.ToString());
        }
        //_connection.AppendToStreamTestAsync("PartitionTest", 99, list);
        var start = 23;
        var end = 22;
        var listRe= await _connection.ReadStreamEventsForwardTestAsync("PartitionTest", start, end);
        
        listRe.Count.ShouldBe(end + 1);
        listRe.First().ShouldBe("23");
        listRe.Last().ShouldBe("45");
      
    }   
    
    [Fact]
    public async Task KvConnectionAllTest()
    {
        KvConnectionAllHelper _connection = new KvConnectionAllHelper();
        
        var re=await _connection.ReadStreamEventsForwardAsync("testlist", 1, 5);
    

      
    }
}


