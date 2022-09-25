using EventStore.ClientAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Orleans.EventSourcing.API.Contracts;
using Orleans.EventSourcing.API.Helpers;
using Orleans.EventSourcing.API.Service;
using EventData = EventStore.ClientAPI.EventData;

namespace Orleans.EventSourcing.API.Controllers;

[ApiController]
[Route("[controller]")]
public class CreateAllController : ControllerBase
{
    private readonly KvConnectionAllHelper _helper=new KvConnectionAllHelper();
    private readonly IKvConnection _kvConnection;
   
    private  List<string> eventDatass = new List<string>(){"w","s","z"};
    private readonly ILogger<EventController> _logger;
    public CreateAllController(IKvConnection kvConnection, ILogger<EventController> logger)
    {
        _kvConnection = kvConnection;
        _logger = logger;
    }

    [HttpGet(Name = "GetCreateAll")]
     public async Task<GetEventsResponse> GetCreate(long start,int count)
     {
      
      try
      {
          //eventDatas.Add(data);
          var re= await   _helper.AppendToStreamAsync("listTest2", count,eventDatass);
          
          return new GetEventsResponse()
          {
              //count = datas.Count
              count = 1
          };
      }
      catch (Exception e)
      {
          Console.WriteLine(e);
          _logger.LogError(e,"GetEvents error!");
          throw;
      }
       
     }
    
    


}

