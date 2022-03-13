using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using MediatR;

namespace ConfiginatorWeb.Interactors.Queries.Section;

public class AddSectionQueryHandler : IRequestHandler<AddSectionIndexQueryRequest, AddSectionIndexQueryResponse>
{
    private readonly EnvironmentDomainService _environmentDomainService;

    public AddSectionQueryHandler(EnvironmentDomainService environmentDomainService)
    {
        _environmentDomainService =
            Guards.HasValue(environmentDomainService, nameof(environmentDomainService));
    }

    public Task<AddSectionIndexQueryResponse> Handle(AddSectionIndexQueryRequest indexQueryRequest,
        CancellationToken cancellationToken)
    {
        var environments = _environmentDomainService
            .EnvironmentTypes
            .SelectMany(et => et.AllowedEnvironments.Select(e => new AddSectionQueryEnvironment(et.EnvironmentTypeName, e)))
            .Select(e => new AddSectionQueryEnvironment(e.EnvironmentType, e.EnvironmentName))
            .ToList();
        return Task.FromResult(new AddSectionIndexQueryResponse(environments));
    }
}

public record AddSectionIndexQueryRequest : IRequest<AddSectionIndexQueryResponse>;

public record AddSectionIndexQueryResponse(List<AddSectionQueryEnvironment> Environments);

public record AddSectionQueryEnvironment(string EnvironmentType, string EnvironmentName);