using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using MediatR;

namespace ConfiginatorWeb.Interactors.Section;

public class GetAddSectionViewHandler : IRequestHandler<GetAddSectionViewModel, AddSectionViewModel>
{
    private readonly EnvironmentValidationService _environmentValidationService;

    public GetAddSectionViewHandler(EnvironmentValidationService environmentValidationService)
    {
        _environmentValidationService =
            Guards.HasValue(environmentValidationService, nameof(environmentValidationService));
    }

    public Task<AddSectionViewModel> Handle(GetAddSectionViewModel viewModel, CancellationToken cancellationToken)
    {
        var environments = _environmentValidationService
            .EnvironmentNames
            .Select(e => new EnvironmentListItem(e.EnvironmentType, e.EnvironmentName))
            .ToList();
        return Task.FromResult(new AddSectionViewModel(environments));
    }
}

public record GetAddSectionViewModel : IRequest<AddSectionViewModel>;

public record AddSectionViewModel(List<EnvironmentListItem> Environments);

public record EnvironmentListItem(string EnvironmentType, string EnvironmentName);