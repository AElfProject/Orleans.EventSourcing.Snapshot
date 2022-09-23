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
public class CreateAllPartitionController : ControllerBase
{
    private readonly KvConnectionPartitionHelper _helper=new KvConnectionPartitionHelper();
    private readonly IKvConnection _kvConnection;
    private  List<string> eventData = new List<string>(){"w","s","z"};
    private readonly ILogger<EventController> _logger;
    public CreateAllPartitionController(IKvConnection kvConnection, ILogger<EventController> logger)
    {
        _kvConnection = kvConnection;
        _logger = logger;
    }

    [HttpGet(Name = "GetCreatePartition")]
     public async Task<GetEventsResponse> GetCreate(long start,int count)
     {
      
      try
      {
          var re= await   _helper.AppendToStreamAsync("listP", count,eventData);
          return new GetEventsResponse()
          {
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

