using Microsoft.AspNetCore.Components;
using RapSuite.Domain.Interfaces;

namespace RapSuite.Components.Pages.Home;

public partial class Home
{
    [Inject] private IUserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (!Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
        }
    }
}
