using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using MediatR;

namespace ConfiginatorWeb.Interactors.Schema;

public class PromoteSectionSchemaCommandHandler : IRequestHandler<PromoteSectionSchemaRequest, PromoteSchemaResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly SectionDomainService _sectionDomainService;

    public PromoteSectionSchemaCommandHandler(SectionDomainService sectionDomainService, IUnitOfWork uow)
    {
        _sectionDomainService = sectionDomainService;
        _uow = uow;
    }

    public async Task<PromoteSchemaResponse> Handle(PromoteSectionSchemaRequest request, CancellationToken cancellationToken)
    {
        var section = await _uow.Sections.GetAsync(new SectionId(request.SectionId), cancellationToken);
        await _sectionDomainService.PromoteSchemaAsync(section, request.SchemaName, request.TargetEnvironmentType, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new PromoteSchemaResponse();
    }
}

public record PromoteSectionSchemaRequest(long SectionId, string SchemaName, string TargetEnvironmentType) : IRequest<PromoteSchemaResponse>;
public record PromoteSchemaResponse;