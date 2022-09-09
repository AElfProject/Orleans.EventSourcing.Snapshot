using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SimpleSample.KV.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public async Task Test1()
    {
        KvConnectionPartitionHelper _connection = new KvConnectionPartitionHelper();
        List<string> list = new List<string> ();
        
        for (int i = 1; i <= 100; i++)
        {
            list.Add(i.ToString());
        }
        //_connection.AppendToStreamTestAsync("PartitionTest", 99, list);
        var start = 23;
        var end = 22;
        var listRe= await _connection.ReadStreamEventsForwardTestAsync("PartitionTest", start, end);
        Assert.AreEqual(listRe.Count, end + 1);
        Assert.AreEqual(listRe.First(), "23"); 
        Assert.AreEqual(listRe.Last(),"45");

      
    }    [Test]
    public async Task Test2()
    {
        KvConnectionListHelper _connection = new KvConnectionListHelper();
        List<string> list = new List<string> ();
        
        for (int i = 1; i <= 10; i++)
        {
            list.Add(i.ToString());
        }
        var re=await _connection.ReadStreamEventsBackwardTestAsync("testlist", 9, 0);
       
        Assert.AreEqual(re.name, "222");
       

      
    }
}


