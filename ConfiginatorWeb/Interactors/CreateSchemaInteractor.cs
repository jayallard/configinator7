using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;

namespace ConfiginatorWeb.Interactors;

public class CreateSchemaInteractor
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSchemaInteractor(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task CreateSchemaAsync(CreateSchemaRequest request)
    {
        // var section = (await _repository.Find(new SectionByName(request.SectionName))).SingleOrDefault();
        // if (section == null) throw new InvalidOperationException("Section doesn't exist");
        // section.AddSchema(request.Schema);
        return Task.CompletedTask;
    }
}