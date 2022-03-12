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

    public static JsonDocument TestSchema()
    {
        //lang=json
        return JsonDocument.Parse(
            "{\n  \"properties\": {\n    \"something\": {\n      \"type\": \"string\"\n    }\n  }\n}");
    }

    public static JsonDocument TestValue()
    {
        return JsonDocument.Parse("{ \"something\": \"value\" }");
    }

    public static DeploymentResult EmptyDeploymentResult()
    {
        return new(true, new List<DeploymentResultMessage>().AsReadOnly());
    }
}