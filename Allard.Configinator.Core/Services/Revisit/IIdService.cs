namespace Allard.Configinator.Core.Services.Revisit;

public interface IIdService
{
    Task<long> GetNextIdAsync(string type);
}