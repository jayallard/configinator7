using Allard.Configinator.Core.Repositories;
using MediatR;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Interactors.Commands.VariableSets;

public class SetVariableValueCommandHandler : IRequestHandler<SetVariableValueCommand, SetVariableValueResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetVariableValueCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SetVariableValueResponse> Handle(SetVariableValueCommand request,
        CancellationToken cancellationToken)
    {
        var variableSet =
            await _unitOfWork.VariableSets.GetVariableSetAsync(request.VariableSetName, cancellationToken);

        // hacky... what's a better way?
        // see if it's a JObject. if so, treat it as one.
        var v = request.Value;
        if (request.Value.Type == JTokenType.String)
        {
            try
            {
                v = JObject.Parse(request.Value.ToString());
            }
            catch
            {
                // not a JObject.
            }
        }
        
        variableSet.SetValue(request.Key, v);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SetVariableValueResponse();
    }
}

public record SetVariableValueCommand
    (string VariableSetName, string Key, JToken Value) : IRequest<SetVariableValueResponse>;

public record SetVariableValueResponse;