using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using MediatR;

namespace ConfiginatorWeb.Interactors.Schema;

public class CreateSectionSchemaInteractor : IRequestHandler<CreateSectionSchemaRequest, CreateSectionSchemaResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly EnvironmentValidationService _environmentValidationService;
    private readonly SectionDomainService _sectionDomainService;

    public CreateSectionSchemaInteractor(EnvironmentValidationService environmentValidationService, IUnitOfWork uow, SectionDomainService sectionDomainService)
    {
        _sectionDomainService = Guards.HasValue(sectionDomainService, nameof(sectionDomainService));
        _environmentValidationService = Guards.HasValue(environmentValidationService, nameof(environmentValidationService));
        _uow = Guards.HasValue(uow, nameof(uow));
    }

    public async Task<CreateSectionSchemaResponse> Handle(CreateSectionSchemaRequest request, CancellationToken cancellationToken)
    {
        var section = await _uow.Sections.GetAsync(new SectionId(request.SectionId), cancellationToken);
        await _sectionDomainService.AddSchemaToSectionAsync(section, request.SchemaName,
            JsonDocument.Parse(request.SchemaText));
        await _uow.SaveChangesAsync(cancellationToken);
        return new CreateSectionSchemaResponse();
    }
}

public record CreateSectionSchemaRequest(long SectionId, string SchemaName, string SchemaText) : IRequest<CreateSectionSchemaResponse>;
public record CreateSectionSchemaResponse;
