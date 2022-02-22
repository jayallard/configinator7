using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using MediatR;

namespace ConfiginatorWeb.Interactors.Schema;

public class CreateSectionSchemaInteractor : IRequestHandler<CreateSchemaRequest, CreateSchemaResponse>
{
    private readonly SchemaDomainService _schemaDomainService;
    private readonly IUnitOfWork _uow;

    public CreateSectionSchemaInteractor(
        IUnitOfWork uow,
        SchemaDomainService schemaDomainService)
    {
        _schemaDomainService = schemaDomainService;
        _uow = Guards.HasValue(uow, nameof(uow));
    }

    public async Task<CreateSchemaResponse> Handle(CreateSchemaRequest request,
        CancellationToken cancellationToken)
    {
        var schema = await _schemaDomainService.CreateSchemaAsync(new SchemaName(request.SchemaName),
            request.SectionId == null ? null : new SectionId(request.SectionId.Value),
            "description - TODO",
            JsonDocument.Parse(request.SchemaText), cancellationToken);
        await _uow.Schemas.AddAsync(schema, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new CreateSchemaResponse();
    }
}

public record CreateSchemaRequest
    (long? SectionId, string SchemaName, string SchemaText) : IRequest<CreateSchemaResponse>
{
    public bool IsGlobal() => SectionId == null;
}

public record CreateSchemaResponse;