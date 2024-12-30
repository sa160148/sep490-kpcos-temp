namespace KPCOS.BusinessLayer.Services;

public interface IRedisPublisher
{
    Task PublishTestEventAsync();
}