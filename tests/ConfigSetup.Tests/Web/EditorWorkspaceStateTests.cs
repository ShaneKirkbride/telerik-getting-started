using ConfigSetup.Web.ViewModels;

namespace ConfigSetup.Tests.Web;

public sealed class EditorWorkspaceStateTests
{
    [Fact]
    public void DefaultsToSourceEditor()
    {
        var state = new EditorWorkspaceState();

        Assert.Equal(EditorWorkspaceTab.SourceEditor, state.ActiveTab);
        Assert.True(state.IsActive(EditorWorkspaceTab.SourceEditor));
        Assert.Equal("nav-link active", state.GetTabCss(EditorWorkspaceTab.SourceEditor));
        Assert.Equal("tab-pane fade show active", state.GetPaneCss(EditorWorkspaceTab.SourceEditor));
    }

    [Fact]
    public void SwitchingToScpiWorkspaceUpdatesCss()
    {
        var state = new EditorWorkspaceState();

        state.SetActiveTab(EditorWorkspaceTab.ScpiWorkspace);

        Assert.True(state.IsActive(EditorWorkspaceTab.ScpiWorkspace));
        Assert.Equal("nav-link", state.GetTabCss(EditorWorkspaceTab.SourceEditor));
        Assert.Equal("nav-link active", state.GetTabCss(EditorWorkspaceTab.ScpiWorkspace));
        Assert.Equal("tab-pane fade", state.GetPaneCss(EditorWorkspaceTab.SourceEditor));
        Assert.Equal("tab-pane fade show active", state.GetPaneCss(EditorWorkspaceTab.ScpiWorkspace));
    }
}
