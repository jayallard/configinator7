using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Tests;

public static class TestUtility
{
    public static JsonDocument EmptyDoc()
    {
        return JsonDocument.Parse("{}");
    }

    public static DeploymentResult EmptyDeploymentResult()
    {
        return new(true, new List<DeploymentResultMessage>().AsReadOnly());
    }
}