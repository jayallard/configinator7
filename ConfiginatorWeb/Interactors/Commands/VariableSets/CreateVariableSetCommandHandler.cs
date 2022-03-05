using System.ComponentModel.DataAnnotations;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using MediatR;

namespace ConfiginatorWeb.Interactors.Commands.VariableSets;

public class CreateVariableSetCommandHandler : IRequestHandler<AddVariableRequest, AddVariableResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly VariableSetDomainService _variableSetDomainService;

    public CreateVariableSetCommandHandler(VariableSetDomainService variableSetDomainService, IUnitOfWork uow)
    {
        _variableSetDomainService = variableSetDomainService;
        _uow = uow;
    }

    public async Task<AddVariableResponse> Handle(AddVariableRequest request, CancellationToken cancellationToken)
    {
        var vs = await _variableSetDomainService.CreateVariableSetAsync(
            request.Namespace,
            request.VariableSetName,
            request.EnvironmentType,
            cancellationToken);
        await _uow.VariableSets.AddAsync(vs, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new AddVariableResponse();
    }
}

public class AddVariableRequest : IRequest<AddVariableResponse>
{
    [Required] public string VariableSetName { get; set; }

    [Required] public string EnvironmentType { get; set; }

    [Required] public string Namespace { get; set; }
}

public record AddVariableResponse;