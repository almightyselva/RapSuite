using Microsoft.AspNetCore.Components;
using RapSuite.Infrastructure.Session;

namespace RapSuite.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] private UserSessionService Session { get; set; } = default!;
}
