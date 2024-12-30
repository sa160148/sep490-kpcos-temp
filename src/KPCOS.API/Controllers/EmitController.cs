using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class EmitController : ControllerBase
{
    private readonly IRedisPublisher _publisher;

    public EmitController(IRedisPublisher publisher)
    {
        _publisher = publisher;
    }

    
    
    [HttpPost("test")]
    public async Task<IActionResult> EmitTest()
    {
        await _publisher.PublishTestEventAsync();
        return Ok(new { status = "Test message emitted successfully." });
    }
}

