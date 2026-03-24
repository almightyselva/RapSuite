using Microsoft.AspNetCore.Components;
using RapSuite.Infrastructure.Session;

namespace RapSuite.Components.Pages.Home;

public partial class Home
{
    [Inject] private UserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (!Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
        }
    }
}
