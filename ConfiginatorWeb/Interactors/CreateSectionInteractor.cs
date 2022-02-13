using System.ComponentModel.DataAnnotations;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Interactors;

public class CreateSectionInteractor : IRequestHandler<CreateSectionAppRequest, CreateSecionAppResponse>
{
    private readonly SectionDomainService _service;
    private readonly IUnitOfWork _uow;


    public CreateSectionInteractor(SectionDomainService service, IUnitOfWork uow)
    {
        _service = service;
        _uow = uow;
    }

    public async Task<CreateSecionAppResponse> Handle(CreateSectionAppRequest request, CancellationToken cancellationToken)
    {
        var section = await _service.CreateSectionAsync(request.Name, request.OrganizationPath);
        
        // todo: this is awkward.
        await _uow.Sections.AddAsync(section, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new CreateSecionAppResponse(section.EntityId);    }
}

public class CreateSectionAppRequest : IRequest<CreateSecionAppResponse>
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string OrganizationPath { get; set; }
    
    [HiddenInput]
    [DataType(DataType.Text)]
    public string? ErrorMessage { get; set; }
}

public record CreateSecionAppResponse(long sectionId);