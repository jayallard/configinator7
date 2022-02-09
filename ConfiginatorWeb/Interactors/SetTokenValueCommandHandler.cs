using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using MediatR;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Interactors;

public class SetTokenValueCommandHandler : IRequestHandler<SetTokenValueCommand, SetTokenValueResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetTokenValueCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SetTokenValueResponse> Handle(SetTokenValueCommand request, CancellationToken cancellationToken)
    {
        var tokenSet = await _unitOfWork.TokenSets.GetTokenSetAsync(request.TokenSetName, cancellationToken);
        tokenSet.SetValue(request.Key, request.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SetTokenValueResponse();
    }
}

public record SetTokenValueCommand(string TokenSetName, string Key, JToken Value) : IRequest<SetTokenValueResponse>;
public record SetTokenValueResponse;