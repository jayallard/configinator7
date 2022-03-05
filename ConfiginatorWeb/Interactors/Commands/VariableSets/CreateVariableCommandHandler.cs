using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using MediatR;

namespace ConfiginatorWeb.Interactors.Commands.VariableSets;

// hack - just slap the variable in, and then let the user hit the edit link to set the value.
// this, and edit, need to be reworked to support create and udpate
public class CreateVariableCommandHandler : IRequestHandler<CreateVariableRequest, CreateVariableResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVariableCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateVariableResponse> Handle(CreateVariableRequest request, CancellationToken cancellationToken)
    {
        var vs = await _unitOfWork.VariableSets.FindOneAsync(new VariableSetNameIs(request.VariableSetName),
            cancellationToken);
        if (vs.ToVariableSet().Variables.ContainsKey(request.Key))
            // todo: move to the domain... breakup set value to add and update
            throw new InvalidOperationException("Variable already exists");

        vs.SetValue(request.Key, string.Empty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateVariableResponse();
    }
}

public class CreateVariableRequest : IRequest<CreateVariableResponse>
{
    public string VariableSetName { get; set; }
    public string Key { get; set; }
}

public record CreateVariableResponse;