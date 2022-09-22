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
public class EventController : ControllerBase
{
    private readonly KvConnectionListHelper _helper=new KvConnectionListHelper();
    private readonly IKvConnection _kvConnection;

    private readonly ILogger<EventController> _logger;
    public EventController(IKvConnection kvConnection, ILogger<EventController> logger)
    {
        _kvConnection = kvConnection;
        _logger = logger;
    }

    [HttpGet(Name = "GetEvents")]
     public async Task<GetEventsResponse> GetEventsByStream(long start,int count)
     {
         
         try
         {
             List<EventData> datas= await  _kvConnection.ReadStreamEventsForwardAsync("listTest", start, 100);
             return new GetEventsResponse()
             {
                 count = 0
             };
         }
         catch (Exception e)
         {
             Console.WriteLine(e);
             _logger.LogError(e,"GetEvents error!");
             throw;
         }
       
     }
    
    [HttpPost(Name = "PostEvents")]
    public async Task<CreateEventsResponse> CreateEvents([FromBody]CreateEventsRequest request)
    {
        /*string starts=AppConfigurtaionServices.Configuration
            .GetSection("RedisSetting:CreateCount").Value;*/
        //long count = request.count;
        long count = 2;
        try
        {
            List<EventData> eventDatas = new List<EventData>();
            for (int i = 0; i < count ;i++)
            {
                EventData data = new EventData( Guid.NewGuid(),i.ToString(),true,null,null);
            
                eventDatas.Add(data);
            }
            var re= await   _helper.AppendToStreamAsync("listTest", count,eventDatas);
            return new CreateEventsResponse()
            {
                IsTrue = re
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.LogError(e,"PostEvents error!");
            throw;
        }
        
    }


}

