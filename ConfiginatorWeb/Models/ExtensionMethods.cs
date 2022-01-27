﻿using Allard.Configinator.Core.Model;
using ConfiginatorWeb.Queries;

namespace ConfiginatorWeb.Models;

public static class ExtensionMethods
{
    public static SectionView ToOutputDto(this SectionEntity section) => new()
    {
        SectionName = section.SectionName,
        Path = section.Path,
        Environments = section.Environments.Select(e => new SectionEnvironmentView
        {
            EnvironmentName = e.EnvironmentName,
            Releases = e.Releases.Select(r => new SectionReleaseView
            {
                Schema = new SectionSchemaView
                {
                    Schema = r.Schema.Schema,
                    Version = r.Schema.Version
                },
                CreateDate = r.CreateDate,
                IsDeployed = r.IsDeployed,
                ReleaseId = r.Id.Id,
                TokenSet = r.TokenSet,
                ModelValue = r.ModelValue,
                ResolvedValue = r.ResolvedValue,
                Deployments = r.Deployments.Select(d => new SectionDeploymentHistoryView
                {
                    DeploymentId = d.Id.Id,
                    DeploymentDate = d.DeploymentDate,
                    RemovedDate = d.RemovedDate,
                    RemoveReason = d.RemoveReason
                }).ToList()
            }).ToList()
        }).ToList(),
        Schemas = section.Schemas.Select(s => new SectionSchemaView
        {
            Schema = s.Schema,
            Version = s.Version
        }).ToList()
    };
}