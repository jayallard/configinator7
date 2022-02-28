using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using MediatR;

namespace ConfiginatorWeb.Interactors.Commands.Section;

public class PromoteSectionCommandHandler : IRequestHandler<PromoteSectionRequest, PromoteSectionResponse>
{
    private readonly SectionDomainService _sectionDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public PromoteSectionCommandHandler(SectionDomainService sectionDomainService, IUnitOfWork unitOfWork)
    {
        _sectionDomainService = sectionDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PromoteSectionResponse> Handle(PromoteSectionRequest request, CancellationToken cancellationToken)
    {
        var section = await _unitOfWork.Sections.GetAsync(new SectionId(request.SectionId), cancellationToken);
        await _sectionDomainService.PromoteToEnvironmentType(section, request.EnvironmentType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new PromoteSectionResponse();
    }
}

public record PromoteSectionRequest(long SectionId, string EnvironmentType) : IRequest<PromoteSectionResponse>;
public record PromoteSectionResponse;