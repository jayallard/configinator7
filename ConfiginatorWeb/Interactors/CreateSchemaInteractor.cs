using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.State;
using Allard.Configinator.Core.Repositories;

namespace ConfiginatorWeb.Interactors;

public class CreateSchemaInteractor
{
    private readonly ISectionRepository _repository;

    public CreateSchemaInteractor(ISectionRepository repository)
    {
        _repository = repository;
    }

    public async Task CreateSchemaAsync(CreateSchemaRequest request)
    {
        var section = await _repository.GetSectionAsync(request.SectionName);
        if (section == null) throw new InvalidOperationException("Section doesn't exist");
        section.AddSchema(request.Schema);
        
    }
}

public record CreateSchemaRequest(string SectionName, SchemaEntity Schema);