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
public class EventEndController : ControllerBase
{
    private readonly KvConnectionListHelper _helper=new KvConnectionListHelper();
    private readonly IKvConnection _kvConnection;

    private readonly ILogger<EventController> _logger;
    public EventEndController(IKvConnection kvConnection, ILogger<EventController> logger)
    {
        _kvConnection = kvConnection;
        _logger = logger;
    }

    [HttpGet(Name = "GetEventEnd")]
     public async Task<GetEventsResponse> GetEventsByStream(long start,int count)
     {

      try
      {
          List<EventData> datas= await  _kvConnection.ReadStreamEventsForwardAsync("listTest", start, 1);
          return new GetEventsResponse()
          {
              count = datas.Count
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

