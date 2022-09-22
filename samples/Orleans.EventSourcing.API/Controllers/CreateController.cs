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
public class CreateController : ControllerBase
{
    private readonly KvConnectionListHelper _helper=new KvConnectionListHelper();
    private readonly IKvConnection _kvConnection;
    private readonly EventData data = new EventData( Guid.NewGuid(),"ee",true,null,null);

    private  List<EventData> eventDatas = new List<EventData>();
    private  List<string> eventDatass = new List<string>(){"dd"};
    private readonly ILogger<EventController> _logger;
    public CreateController(IKvConnection kvConnection, ILogger<EventController> logger)
    {
        _kvConnection = kvConnection;
        _logger = logger;
    }

    [HttpGet(Name = "GetCreate")]
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

