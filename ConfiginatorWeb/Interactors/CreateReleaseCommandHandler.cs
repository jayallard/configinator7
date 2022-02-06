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
    private readonly SectionDomainService _sectionDomainService;

    public CreateReleaseCommandHandler(IUnitOfWork unitOfWork, IIdentityService identity,
        TokenSetDomainService tokenSetDomainService, SectionDomainService sectionDomainService)
    {
        _unitOfWork = unitOfWork;
        _identity = identity;
        _tokenSetDomainService = tokenSetDomainService;
        _sectionDomainService = sectionDomainService;
    }

    public async Task<CreateReleaseResponse> Handle(CreateReleaseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var section =
                (await _unitOfWork.Sections.FindAsync(new SectionNameIs(request.SectionName), cancellationToken))
                .Single();
            var environmentId = section.GetEnvironment(request.EnvironmentName).Id;
            var tokenSetId =
                request.TokenSetName == null
                    ? null
                    : (await _unitOfWork.TokenSets.FindAsync(new TokenSetNameIs(request.TokenSetName),
                        cancellationToken))
                    .Single().Id;
            var schemaId = section.GetSchema(SemanticVersion.Parse(request.SchemaVersion)).Id;
            await _sectionDomainService.CreateReleaseAsync(
                section,
                environmentId,
                tokenSetId,
                schemaId,
                JsonDocument.Parse(request.Value), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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