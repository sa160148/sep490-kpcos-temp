/*using KPCOS.Common;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace KPCOS.BusinessLayer.Services.Implements;

public class RedisPublisher : IRedisPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubscriber _subscriber;
    private readonly ILogger<RedisPublisher> _logger;
    private readonly SocketIoEmitter socketIo;
  

    public RedisPublisher(IConnectionMultiplexer redis, ILogger<RedisPublisher> logger, SocketIoEmitter socketIo)
    {
        _redis = redis;
        _subscriber = _redis.GetSubscriber();
        _logger = logger;
        this.socketIo = socketIo;
    }
    public async Task PublishTestEventAsync()
    {
        socketIo.EmitToRoomAsync("test_room", "test_event", "This is a test message");
    }


}*/