using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using MediatR;

namespace ConfiginatorWeb.Interactors.Schema;

public class CreateSectionSchemaInteractor : IRequestHandler<CreateSectionSchemaRequest, CreateSectionSchemaResponse>
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

    public async Task<CreateSectionSchemaResponse> Handle(CreateSectionSchemaRequest request,
        CancellationToken cancellationToken)
    {
        var schema = await _schemaDomainService.CreateSectionSchemaAsync(new SchemaName(request.SchemaName),
            new SectionId(request.SectionId),
            "description - TODO",
            JsonDocument.Parse(request.SchemaText), cancellationToken);
        await _uow.Schemas.AddAsync(schema, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new CreateSectionSchemaResponse();
    }
}

public record CreateSectionSchemaRequest
    (long SectionId, string SchemaName, string SchemaText) : IRequest<CreateSectionSchemaResponse>;

public record CreateSectionSchemaResponse;