using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using ConfiginatorWeb.Controllers;
using MediatR;

namespace ConfiginatorWeb.Interactors;

public class ReleaseDeployCommandHandler : IRequestHandler<ReleaseDeployRequest, ReleaseDeployResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ReleaseDeployCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReleaseDeployResponse> Handle(ReleaseDeployRequest request, CancellationToken cancellationToken)
    {
        var section = (await _unitOfWork.Sections.FindAsync(new SectionNameIs(request.SectionName))).Single();
        var env = section.GetEnvironment(request.EnvironmentName);
        var release = env.GetRelease(new ReleaseId(request.ReleaseId));
        
        // todo: id
        release.SetDeployed(new DeploymentHistoryId(20), DateTime.Now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new ReleaseDeployResponse();
    }
}