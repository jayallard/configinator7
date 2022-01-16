namespace Allard.Bus;

public interface ISubscriber
{
    Task Subscribe<T>(Action<T> execute);
}