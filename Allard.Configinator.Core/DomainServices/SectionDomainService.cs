using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;

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

    public async Task<SectionEntity> CreateSectionAsync(string sectionName, string path)
    {
        // make sure section doesn't already exist
        if (await _unitOfWork.Exists(new SectionNameIs(sectionName)))
        {
            throw new InvalidOperationException("Section already exists: " + sectionName);
        }

        if (await _unitOfWork.Exists(new PathIs(path)))
        {
            throw new InvalidOperationException("The path is already in use by another section");
        }

        // todo: might as well make sure the id is unique too

        var id = await _identityService.GetId<SectionId>();
        var section = new SectionEntity(id, sectionName, path, null, null);
        await _unitOfWork.AddSectionAsync(section);
        return section;
    }

    public async Task<TokenSetEntity> CreateTokenSetAsync(string tokenSetName, string? baseTokenSetName)
    {
        if (await _unitOfWork.Exists(new TokenSetNameIs(tokenSetName)))
        {
            throw new InvalidOperationException("TokenSet already exists: " + tokenSetName);
        }

        if (baseTokenSetName is not null && !(await _unitOfWork.Exists(new TokenSetNameIs(baseTokenSetName))))
        {
            throw new InvalidOperationException("Base TokenSet doesn't exist: " + baseTokenSetName);
        }

        var id = await _identityService.GetId<TokenSetId>();
        return new TokenSetEntity(id, tokenSetName, baseTokenSetName);
    }
}