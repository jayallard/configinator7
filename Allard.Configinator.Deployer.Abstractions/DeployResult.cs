using System.Collections.ObjectModel;

namespace Allard.Configinator.Deployer.Abstractions;

public class DeployResult
{
    private readonly List<DeployResultMessage> _messages = new();

    public ReadOnlyCollection<DeployResultMessage> Messages => _messages.AsReadOnly();

    public bool IsSuccess => _messages.Any() && _messages.All(m => m.Severity != ResultSeverity.Error);
    
    public DeployResult AddError(string source, string key, string message, Exception? exception = null)
    {
        _messages.Add(new DeployResultMessage(source, key, ResultSeverity.Error, message, exception));
        return this;
    }

    public DeployResult AddInformation(string source, string key, string message)
    {
        _messages.Add(new DeployResultMessage(source, key, ResultSeverity.Information, message));
        return this;
    }
    
    public DeployResult AddWarning(string source, string key, string message, Exception exception = null)
    {
        _messages.Add(new DeployResultMessage(source, key, ResultSeverity.Warning, message));
        return this;
    }

}

public record DeployResultMessage(string Source, string Key, ResultSeverity Severity, string Message, Exception? Exception = null);

public enum ResultSeverity
{
    Information,
    Warning,
    Error
}