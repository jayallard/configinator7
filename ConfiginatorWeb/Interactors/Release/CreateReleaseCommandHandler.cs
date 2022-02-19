using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications.Schema;
using MediatR;

namespace ConfiginatorWeb.Interactors.Release;

public class CreateReleaseCommandHandler : IRequestHandler<CreateReleaseRequest, CreateReleaseResponse>
{
    private readonly ILogger<CreateReleaseCommandHandler> _logger;
    private readonly SectionDomainService _sectionDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        SectionDomainService sectionDomainService,
        ILogger<CreateReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _sectionDomainService = sectionDomainService;
        _logger = logger;
    }

    public async Task<CreateReleaseResponse> Handle(CreateReleaseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var section = await _unitOfWork.Sections.GetSectionAsync(request.SectionName, cancellationToken);
            var environmentId = section.GetEnvironment(request.EnvironmentName).Id;
            var variableSet =
                await _unitOfWork.VariableSets.GetVariableSetIfNotNullAsync(request.VariableSetName, cancellationToken);
            var schemaId =
                (await _unitOfWork.Schemas.FindOneAsync(SchemaNameIs.Is(request.SchemaName), cancellationToken))
                .Id;
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
            _logger.LogError(vex, "Schema Validation Failed");
            var errors = vex.Errors.Select(v => v.ToString()).ToList();
            return new CreateReleaseResponse(false, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Boom");
            return new CreateReleaseResponse(false, new List<string> {ex.Message});
        }
    }
}

public class CreateReleaseRequest : IRequest<CreateReleaseResponse>
{
    [Required] public string EnvironmentName { get; set; }

    [Required] public string SchemaName { get; set; }

    [Required] public string Value { get; set; }

    public string? VariableSetName { get; set; }

    [Required] public string SectionName { get; set; }
}

public record CreateReleaseResponse(bool Success, List<string> ErrorMessages);