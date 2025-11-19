using ConfigSetup.Web.ViewModels;

namespace ConfigSetup.Tests.Web;

public sealed class ScpiWorkspaceStateTests
{
    [Fact]
    public void DefaultsToCommandTab()
    {
        var state = new ScpiWorkspaceState();

        Assert.Equal(ScpiWorkspaceTab.Commands, state.ActiveTab);
        Assert.False(state.IsInstrumentPanelVisible);
        Assert.Equal("nav-link active", state.GetTabCss(ScpiWorkspaceTab.Commands));
        Assert.Equal("tab-pane fade show active", state.GetPaneCss(ScpiWorkspaceTab.Commands));
    }

    [Fact]
    public void SwitchingTabsMakesInstrumentPanelVisible()
    {
        var state = new ScpiWorkspaceState();

        state.SetActiveTab(ScpiWorkspaceTab.Instrument);

        Assert.True(state.IsInstrumentPanelVisible);
        Assert.True(state.IsActive(ScpiWorkspaceTab.Instrument));
        Assert.Equal("nav-link", state.GetTabCss(ScpiWorkspaceTab.Commands));
        Assert.Equal("nav-link active", state.GetTabCss(ScpiWorkspaceTab.Instrument));
        Assert.Equal("tab-pane fade", state.GetPaneCss(ScpiWorkspaceTab.Commands));
        Assert.Equal("tab-pane fade show active", state.GetPaneCss(ScpiWorkspaceTab.Instrument));
    }
}
