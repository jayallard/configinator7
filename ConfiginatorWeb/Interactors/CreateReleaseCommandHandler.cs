using System.Text.Json;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using MediatR;
using NuGet.Versioning;

namespace ConfiginatorWeb.Interactors;

public class CreateReleaseCommandHandler : IRequestHandler<CreateReleaseRequest, CreateReleaseResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SectionDomainService _sectionDomainService;

    public CreateReleaseCommandHandler(
        IUnitOfWork unitOfWork, 
        SectionDomainService sectionDomainService)
    {
        _unitOfWork = unitOfWork;
        _sectionDomainService = sectionDomainService;
    }

    public async Task<CreateReleaseResponse> Handle(CreateReleaseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var section = await _unitOfWork.Sections.GetSectionAsync(request.SectionName, cancellationToken);
            var environmentId = section.GetEnvironment(request.EnvironmentName).Id;
            var variableSet = await _unitOfWork.VariableSets.GetVariableSetIfNotNullAsync(request.VariableSetName, cancellationToken);
            var schemaId = section.GetSchema(SemanticVersion.Parse(request.SchemaVersion)).Id;
            await _sectionDomainService.CreateReleaseAsync(
                section,
                environmentId,
                variableSet?.Id,
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
    public string? VariableSetName { get; set; }
    public string SectionName { get; set; }
}

public record CreateReleaseResponse(bool Success, List<string> ErrorMessages);