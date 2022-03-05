using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using MediatR;

namespace ConfiginatorWeb.Interactors.Commands.Schema;

/// <summary>
///     Create a schema.
///     The schema is created within a namespace.
///     It may be assigned to a configuration section, or not.
///     If not, it is a schema that may be referred to by other schemas
///     within the same namespace or any of its ascendants.
/// </summary>
public class CreateSchemaCommandHandler : IRequestHandler<CreateSchemaRequest, CreateSchemaResponse>
{
    private readonly SchemaDomainService _schemaDomainService;
    private readonly IUnitOfWork _uow;

    public CreateSchemaCommandHandler(
        IUnitOfWork uow,
        SchemaDomainService schemaDomainService)
    {
        _schemaDomainService = Guards.HasValue(schemaDomainService, nameof(schemaDomainService));
        _uow = Guards.HasValue(uow, nameof(uow));
    }

    public async Task<CreateSchemaResponse> Handle(CreateSchemaRequest request,
        CancellationToken cancellationToken)
    {
        var schema = await _schemaDomainService.CreateSchemaAsync(request.SectionId, request.Namespace,
            new SchemaName(request.SchemaName),
            "description - TODO",
            JsonDocument.Parse(request.SchemaText), cancellationToken);
        await _uow.Schemas.AddAsync(schema, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new CreateSchemaResponse();
    }
}

public record CreateSchemaRequest
    (string Namespace, string SchemaName, string SchemaText, SectionId? SectionId) : IRequest<CreateSchemaResponse>;

public record CreateSchemaResponse;