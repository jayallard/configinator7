﻿using System.Text.Json;

namespace Allard.Configinator.Deployer.Abstractions;

/// <summary>
/// All of the information that a deployer needs to deploy.
/// </summary>
public class DeployRequest
{
    public JsonDocument ResolvedValue { get; set; }
    
    public Schema Schema { get; set; }
    public Section Section { get; set; }
    public Environment Environment { get; set; }
    public Release Release { get; set;}
    public Deployment Deployment { get; set; }
}

public record Schema(JsonDocument ResolvedSchema);
public record Section(long SectionId, string SectionName);
public record Environment(long EnvironmentId, string EnvironmentName);
public record Release(long ReleaseId);
public record Deployment(long DeploymentId, string Notes);
