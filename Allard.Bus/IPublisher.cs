namespace Allard.Bus;

public interface IPublisher
{
    Task Publish<T>(T message);
}