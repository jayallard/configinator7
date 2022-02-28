using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications.Schema;
using MediatR;

namespace ConfiginatorWeb.Interactors.Commands.Schema;

/// <summary>
/// Promote a schema from one environment type to another.
/// IE: dev -> staging -> production
/// </summary>
public class PromoteSectionSchemaCommandHandler : IRequestHandler<PromoteSchemaRequest, PromoteSchemaResponse>
{
    private readonly SchemaDomainService _schemaDomainService;
    private readonly IUnitOfWork _uow;

    public PromoteSectionSchemaCommandHandler(
        IUnitOfWork uow,
        SchemaDomainService schemaDomainService)
    {
        _uow = Guards.HasValue(uow, nameof(uow));
        _schemaDomainService = Guards.HasValue(schemaDomainService, nameof(schemaDomainService));
    }

    public async Task<PromoteSchemaResponse> Handle(PromoteSchemaRequest request, CancellationToken cancellationToken)
    {
        var schema = await _uow.Schemas.FindOneAsync(SchemaNameIs.Is(request.SchemaName), cancellationToken);
        await _schemaDomainService.PromoteSchemaAsync(new SchemaName(request.SchemaName), request.TargetEnvironmentType, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new PromoteSchemaResponse(schema.EntityId, request.SchemaName);
    }
}

public record PromoteSchemaRequest(string SchemaName, string TargetEnvironmentType) : IRequest<PromoteSchemaResponse>;

public record PromoteSchemaResponse(long SchemaId, string SchemaName);