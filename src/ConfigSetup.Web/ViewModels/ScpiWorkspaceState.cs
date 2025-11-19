namespace ConfigSetup.Web.ViewModels;

/// <summary>
/// Maintains the visibility state of the SCPI workspace tabs.
/// </summary>
public sealed class ScpiWorkspaceState
{
    public ScpiWorkspaceTab ActiveTab { get; private set; } = ScpiWorkspaceTab.Commands;

    public bool IsInstrumentPanelVisible => ActiveTab == ScpiWorkspaceTab.Instrument;

    public void SetActiveTab(ScpiWorkspaceTab tab)
    {
        ActiveTab = tab;
    }

    public bool IsActive(ScpiWorkspaceTab tab) => ActiveTab == tab;

    public string GetTabCss(ScpiWorkspaceTab tab)
    {
        return IsActive(tab) ? "nav-link active" : "nav-link";
    }

    public string GetPaneCss(ScpiWorkspaceTab tab)
    {
        const string baseCss = "tab-pane fade";
        return IsActive(tab) ? $"{baseCss} show active" : baseCss;
    }
}
