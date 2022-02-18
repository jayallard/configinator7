using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Deployer.Abstractions;
using ConfiginatorWeb.Controllers;
using MediatR;

namespace ConfiginatorWeb.Interactors.Release;

public class DeployCommandHandler : IRequestHandler<HttpDeployRequest, DeployResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IDeployerFactory _deployerFactory;

    public DeployCommandHandler(IUnitOfWork unitOfWork, IIdentityService identityService, IDeployerFactory deployerFactory)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _deployerFactory = deployerFactory;
    }

    public async Task<DeployResponse> Handle(HttpDeployRequest request, CancellationToken cancellationToken)
    {
        // todo: move to domain service. this is inconsistent with the creates 
        var section = await _unitOfWork.Sections.GetAsync(new SectionId(request.SectionId), cancellationToken);
        var env = section.GetEnvironment(new EnvironmentId(request.EnvironmentId));
        var release = env.Releases.GetRelease(new ReleaseId(request.ReleaseId));
        var deploymentId = await _identityService.GetId<DeploymentId>();

        var deployRequest = new DeployRequest
        {
            Deployment = new Deployment(deploymentId.Id, request.Notes),
            DeploymentEnvironment = new DeploymentEnvironment(env.Id.Id, env.EnvironmentName),
            Release = new Allard.Configinator.Deployer.Abstractions.Release(release.Id.Id),
            Schema = new Allard.Configinator.Deployer.Abstractions.Schema(release.SectionSchema.Schema),
            Section = new Allard.Configinator.Deployer.Abstractions.Section(section.Id.Id, section.SectionName),
            ResolvedValue = release.ResolvedValue
        };

        var startDate = DateTime.Now;
        var deployer = await _deployerFactory.GetDeployer(deployRequest);
        var result = await deployer.DeployAsync(deployRequest, cancellationToken);
        var coreResult = new DeploymentResult(result.IsSuccess,
            result.Messages
                .Select(m => new DeploymentResultMessage(m.Source, m.Key, m.Severity, m.Message, m.Exception)).ToList()
                .AsReadOnly());
        
        // todo: convert start date and notes to an object
        section.SetDeployed(env.Id, new ReleaseId(request.ReleaseId), deploymentId, coreResult, startDate, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeployResponse(deploymentId.Id);
    }
}