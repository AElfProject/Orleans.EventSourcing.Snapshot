using Microsoft.AspNetCore.Mvc;
using Orleans.EventSourcing.API.Contracts;

namespace Orleans.EventSourcing.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController: ControllerBase
{
    [HttpGet(Name = "GetTest")]
    public async Task<CreateEventsResponse> GetEventsByStream(long start,int count)
    {
        // return await  _helper.ReadStreamEventsForwardAsync("listTest", start, count);
        return new CreateEventsResponse()
        {
            IsTrue = true
        };


    }
}