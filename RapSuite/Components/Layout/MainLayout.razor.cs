using Microsoft.AspNetCore.Components;
using RapSuite.Domain.Interfaces;

namespace RapSuite.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] private IUserSessionService Session { get; set; } = default!;
}
