namespace Allard.Configinator.Core.Services;

public interface IIdService
{
    Task<long> GetNextIdAsync(string type);
}