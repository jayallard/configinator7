using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.State;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Services;

namespace Allard.Configinator.Core.Interactors;

public class CreateSectionInteractor
{
    private readonly IUnitOfWork _uow;
    private readonly IDomainServices _domainServices;
    private readonly IIdService _idService;

    public CreateSectionInteractor(IIdService idService, IDomainServices domainServices, IUnitOfWork uow)
    {
        _idService = idService;
        _domainServices = domainServices;
        _uow = uow;
    }

    public async Task CreateSection(CreateSectionRequest request)
    {
        var (sectionName, path, schema) = request;
        await _domainServices.EnsureSectionDoesntExistAsync(sectionName);

        var id = await _idService.GetNextIdAsync("section");
        var section = new SectionEntity(new SectionId(id), sectionName, path);
        if (schema != null) section.AddSchema(schema);
        await _uow.Sections.AddSectionAsync(section);
        await _uow.SaveAsync();
    }
}

public record CreateSectionRequest(string SectionName, string Path, ConfigurationSchema? Schema = null);