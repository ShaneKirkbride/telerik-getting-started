namespace ConfigSetup.Web.ViewModels;

public sealed class EditorWorkspaceState
{
    public EditorWorkspaceTab ActiveTab { get; private set; } = EditorWorkspaceTab.SourceEditor;

    public void SetActiveTab(EditorWorkspaceTab tab)
    {
        ActiveTab = tab;
    }

    public bool IsActive(EditorWorkspaceTab tab)
    {
        return ActiveTab == tab;
    }

    public string GetTabCss(EditorWorkspaceTab tab)
    {
        return IsActive(tab) ? "nav-link active" : "nav-link";
    }

    public string GetPaneCss(EditorWorkspaceTab tab)
    {
        return IsActive(tab) ? "tab-pane fade show active" : "tab-pane fade";
    }
}
