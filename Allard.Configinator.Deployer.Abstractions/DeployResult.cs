using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace Allard.Configinator.Deployer.Abstractions;

public class DeployResult
{
    private readonly List<DeployResultMessage> _messages = new();

    public ReadOnlyCollection<DeployResultMessage> Messages => _messages.AsReadOnly();

    public bool IsSuccess => _messages.Any() && _messages.All(m => m.Severity != LogLevel.Error);

    public DeployResult AddError(string source, string key, string message, string? exception = null)
    {
        _messages.Add(new DeployResultMessage(source, key, LogLevel.Error, message, exception));
        return this;
    }

    public DeployResult AddInformation(string source, string key, string message)
    {
        _messages.Add(new DeployResultMessage(source, key, LogLevel.Information, message));
        return this;
    }

    public DeployResult AddWarning(string source, string key, string message, Exception exception = null)
    {
        _messages.Add(new DeployResultMessage(source, key, LogLevel.Warning, message));
        return this;
    }
}

public record DeployResultMessage(string Source, string Key, LogLevel Severity, string Message,
    string? Exception = null);