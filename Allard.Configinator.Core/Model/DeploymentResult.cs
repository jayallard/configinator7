using Microsoft.Extensions.Logging;

namespace Allard.Configinator.Core.Model;

public record DeploymentResult(
    bool IsSuccess, 
    IReadOnlyCollection<DeploymentResultMessage> Messages);

public record DeploymentResultMessage(string Source, string Key, LogLevel Severity, string Message, Exception? Exception = null);
