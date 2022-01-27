using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using ConfiginatorWeb.Controllers;
using MediatR;

namespace ConfiginatorWeb.Interactors;

public class ReleaseDeployCommandHandler : IRequestHandler<ReleaseDeployRequest, ReleaseDeployResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public ReleaseDeployCommandHandler(IUnitOfWork unitOfWork, IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<ReleaseDeployResponse> Handle(ReleaseDeployRequest request, CancellationToken cancellationToken)
    {
        var section = (await _unitOfWork.Sections.FindAsync(new SectionNameIs(request.SectionName))).Single();
        var env = section.GetEnvironment(request.EnvironmentName);
        var deploymentHistoryId = await _identityService.GetId<DeploymentHistoryId>();
        section.SetDeployed(env.Id, new ReleaseId(request.ReleaseId), deploymentHistoryId, DateTime.Now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new ReleaseDeployResponse();
    }
}