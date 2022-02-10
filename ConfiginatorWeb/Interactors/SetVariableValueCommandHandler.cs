using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using MediatR;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Interactors;

public class SetVariableValueCommandHandler : IRequestHandler<SetVariableValueCommand, SetVariableValueResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetVariableValueCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SetVariableValueResponse> Handle(SetVariableValueCommand request, CancellationToken cancellationToken)
    {
        var variableSet = await _unitOfWork.VariableSets.GetVariableSetAsync(request.VariableSetName, cancellationToken);
        variableSet.SetValue(request.Key, request.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SetVariableValueResponse();
    }
}

public record SetVariableValueCommand(string VariableSetName, string Key, JToken Value) : IRequest<SetVariableValueResponse>;
public record SetVariableValueResponse;