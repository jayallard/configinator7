using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;

namespace Allard.Configinator.Core.DomainServices;

public class SectionDomainService
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public SectionDomainService(IIdentityService identityService, IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<SectionEntity> CreateSectionAsync(string name, string path)
    {
        // make sure section doesn't already exist
        if (await _unitOfWork.Exists(new SectionNameIs(name)))
        {
            throw new InvalidOperationException("Section already exists: " + name);
        }

        if (await _unitOfWork.Exists(new PathIs(path)))
        {
            throw new InvalidOperationException("The path is already in use by another section");
        }
        
        // todo: might as well make sure the id is unique too

        var id = await _identityService.GetId<SectionId>();
        var section = new SectionEntity(id, name, path, null, null);
        await _unitOfWork.AddSectionAsync(section);
        return section;
    }
}