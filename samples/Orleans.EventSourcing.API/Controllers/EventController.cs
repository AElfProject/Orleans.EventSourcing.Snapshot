using EventStore.ClientAPI;
using Microsoft.AspNetCore.Mvc;
using Orleans.EventSourcing.API.Contracts;
using Orleans.EventSourcing.KV;

namespace Orleans.EventSourcing.API.Controllers;

[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly KvConnectionListHelper _helper=new KvConnectionListHelper();

    [HttpGet(Name = "GetEvents")]
     public async Task<List<EventData>> GetEventsByStream(long start,int count)
     {
       return await  _helper.ReadStreamEventsForwardAsync("listTest", start, count);
         
     }
    
    [HttpPost(Name = "PostEvents")]
    public async Task<CreateEventsResponse> CreateEvents(long count)
    {
        List<EventData> eventDatas = new List<EventData>();
        for (int i = 0; i < count; i++)
        {
            EventData data = new EventData( Guid.NewGuid(),i.ToString(),true,null,null);
            
            eventDatas.Add(data);
        }
        var re= await   _helper.AppendToStreamAsync("listTest", 1000,eventDatas);
        return new CreateEventsResponse();
    }


}

