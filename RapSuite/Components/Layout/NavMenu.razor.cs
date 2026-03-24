using Microsoft.AspNetCore.Components;
using RapSuite.Infrastructure.Session;

namespace RapSuite.Components.Layout;

public partial class NavMenu
{
    [Inject] private UserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private void Logout()
    {
        Session.ClearUser();
        Navigation.NavigateTo("/login");
    }
}
