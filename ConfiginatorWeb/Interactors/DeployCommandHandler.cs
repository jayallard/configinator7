using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using ConfiginatorWeb.Controllers;
using MediatR;

namespace ConfiginatorWeb.Interactors;

public class DeployCommandHandler : IRequestHandler<DeployRequest, DeployResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public DeployCommandHandler(IUnitOfWork unitOfWork, IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<DeployResponse> Handle(DeployRequest request, CancellationToken cancellationToken)
    {
        // todo: move to domain service. this is inconsistent with the creates 
        var section = await _unitOfWork.Sections.GetAsync(new SectionId(request.SectionId), cancellationToken);
        var env = section.GetEnvironment(new EnvironmentId(request.EnvironmentId));
        var deploymentHistoryId = await _identityService.GetId<DeploymentId>();
        section.SetDeployed(env.Id, new ReleaseId(request.ReleaseId), deploymentHistoryId, DateTime.Now, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeployResponse(deploymentHistoryId.Id);
    }
}