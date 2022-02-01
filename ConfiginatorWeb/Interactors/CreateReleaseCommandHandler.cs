using System.Text.Json;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using MediatR;
using NuGet.Versioning;

namespace ConfiginatorWeb.Interactors;

public class CreateReleaseCommandHandler : IRequestHandler<CreateReleaseRequest, CreateReleaseResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identity;
    private readonly TokenSetDomainService _tokenSetDomainService;

    public CreateReleaseCommandHandler(IUnitOfWork unitOfWork, IIdentityService identity, TokenSetDomainService tokenSetDomainService)
    {
        _unitOfWork = unitOfWork;
        _identity = identity;
        _tokenSetDomainService = tokenSetDomainService;
    }

    public async Task<CreateReleaseResponse> Handle(CreateReleaseRequest request, CancellationToken cancellationToken)
    {
        // todo  convert to domain service
        try
        {
            var json = JsonDocument.Parse(request.Value);
            var section =
                (await _unitOfWork.Sections.FindAsync(new SectionNameIs(request.SectionName), cancellationToken))
                .Single();
            var tokens =
                request.TokenSetName == null
                ? null
                : await _tokenSetDomainService.GetTokenSetComposedAsync(request.TokenSetName, cancellationToken);

            var releaseId = await _identity.GetId<ReleaseId>();
            var env = section.GetEnvironment(request.EnvironmentName);
            var schema = section.GetSchema(SemanticVersion.Parse(request.SchemaVersion));
            await section.CreateReleaseAsync(env.Id, releaseId, tokens, schema.Id, json, cancellationToken);
            return new CreateReleaseResponse(true, new List<string>());
        }
        catch (SchemaValidationFailedException vex)
        {
            var errors = vex.Errors.Select(v => v.ToString()).ToList();
            return new CreateReleaseResponse(false, errors);
        }
        catch (Exception ex)
        {
            return new CreateReleaseResponse(false, new List<string> {ex.Message});
        }
    }
}

public class CreateReleaseRequest : IRequest<CreateReleaseResponse>
{
    public string EnvironmentName { get; set; }
    public string SchemaVersion { get; set; }
    public string Value { get; set; }
    public string? TokenSetName { get; set; }
    public string SectionName { get; set; }
}

public record CreateReleaseResponse(bool Success, List<string> ErrorMessages);
