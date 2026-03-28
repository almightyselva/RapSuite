using Microsoft.AspNetCore.Components;
using RapSuite.Domain.Interfaces;

namespace RapSuite.Components.Layout;

public partial class NavMenu
{
    [Inject] private IUserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private void Logout()
    {
        Session.ClearUser();
        Navigation.NavigateTo("/login");
    }

    private void SignIn()
    {
        Session.ClearUser();
        Navigation.NavigateTo("/login");
    }
}
