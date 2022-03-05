using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using MediatR;

namespace ConfiginatorWeb.Interactors.Queries.Section;

public class AddSectionQueryHandler : IRequestHandler<AddSectionIndexQueryRequest, AddSectionIndexQueryResponse>
{
    private readonly EnvironmentValidationService _environmentValidationService;

    public AddSectionQueryHandler(EnvironmentValidationService environmentValidationService)
    {
        _environmentValidationService =
            Guards.HasValue(environmentValidationService, nameof(environmentValidationService));
    }

    public Task<AddSectionIndexQueryResponse> Handle(AddSectionIndexQueryRequest indexQueryRequest,
        CancellationToken cancellationToken)
    {
        var environments = _environmentValidationService
            .EnvironmentNames
            .Select(e => new AddSectionQueryEnvironment(e.EnvironmentType, e.EnvironmentName))
            .ToList();
        return Task.FromResult(new AddSectionIndexQueryResponse(environments));
    }
}

public record AddSectionIndexQueryRequest : IRequest<AddSectionIndexQueryResponse>;

public record AddSectionIndexQueryResponse(List<AddSectionQueryEnvironment> Environments);

public record AddSectionQueryEnvironment(string EnvironmentType, string EnvironmentName);