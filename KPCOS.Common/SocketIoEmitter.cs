/*using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace KPCOS.Common;

public class SocketIoEmitter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubscriber _subscriber;
    private readonly ILogger<SocketIoEmitter> _logger;

    public SocketIoEmitter(IConnectionMultiplexer redis, ILogger<SocketIoEmitter> logger)
    {
        _redis = redis;
        _subscriber = _redis.GetSubscriber();
        _logger = logger;
    }
   
    
    public async Task EmitToRoomAsync( string roomId, string eventName, object data)
    {
        const string channel = "my_channel"; 
        var payload = new
        {
            Event = eventName,
            Room =  roomId,
            Message = data,
        };
        string message = JsonSerializer.Serialize(payload);
        
        if (!_redis.IsConnected)
        {
            _logger.LogError("Redis connection is not connected.");
            return;
        }
        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(channel, message);
        Console.WriteLine($"Published message: '{message}' to channel: '{channel}'");
    }
}*/