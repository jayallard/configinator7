using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using MediatR;

namespace ConfiginatorWeb.Interactors.Commands.Section;

/// <summary>
///     Add an environment to a section.
/// </summary>
public class
    AddEnvironmentsToSectionCommandHandler : IRequestHandler<AddEnvironmentsToSectionRequest,
        AddEnvironmentToSectionResponse>
{
    private readonly SectionDomainService _sectionDomainService;
    private readonly IUnitOfWork _uow;

    public AddEnvironmentsToSectionCommandHandler(SectionDomainService sectionDomainService, IUnitOfWork uow)
    {
        _sectionDomainService = Guards.HasValue(sectionDomainService, nameof(sectionDomainService));
        _uow = Guards.HasValue(uow, nameof(uow));
    }

    public async Task<AddEnvironmentToSectionResponse> Handle(AddEnvironmentsToSectionRequest request,
        CancellationToken cancellationToken)
    {
        var section = await _uow.Sections.GetAsync(new SectionId(request.SectionId), cancellationToken);
        foreach (var e in request.EnvironmentNames)
            await _sectionDomainService.AddEnvironmentToSectionAsync(section, e);

        await _uow.SaveChangesAsync(cancellationToken);
        return new AddEnvironmentToSectionResponse();
    }
}

public record AddEnvironmentsToSectionRequest
    (long SectionId, List<string> EnvironmentNames) : IRequest<AddEnvironmentToSectionResponse>;

public record AddEnvironmentToSectionResponse;