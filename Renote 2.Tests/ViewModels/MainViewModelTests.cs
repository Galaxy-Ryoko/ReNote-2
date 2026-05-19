using Renote_2.ViewModels;
using Xunit;

namespace Renote_2.Tests.ViewModels;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_CreatesInitialDocument()
    {
        var vm = new MainViewModel();

        Assert.Single(vm.Documents);
        Assert.NotNull(vm.ActiveDocument);
        Assert.Equal(0, vm.SelectedTabIndex);
    }

    [Fact]
    public void CreateNewDocument_AddsTabAndSelectsIt()
    {
        var vm = new MainViewModel();
        vm.CreateNewDocument();

        Assert.Equal(2, vm.Documents.Count);
        Assert.Equal(1, vm.SelectedTabIndex);
        Assert.Same(vm.Documents[1], vm.ActiveDocument);
    }

    [Fact]
    public void ZoomIn_UpdatesEditorFontSize()
    {
        var vm = new MainViewModel { ZoomLevel = 100 };
        vm.ZoomIn();

        Assert.Equal(110, vm.ZoomLevel);
        Assert.Equal(13.2, vm.EditorFontSize, precision: 1);
    }

    [Fact]
    public void ZoomOut_DoesNotGoBelowMinimum()
    {
        var vm = new MainViewModel { ZoomLevel = 50 };
        vm.ZoomOut();

        Assert.Equal(50, vm.ZoomLevel);
    }

    [Fact]
    public void RemoveDocument_SelectsAnotherTab_WhenActiveClosed()
    {
        var vm = new MainViewModel();
        var first = vm.ActiveDocument!;
        vm.CreateNewDocument();
        var second = vm.ActiveDocument!;

        vm.RemoveDocument(second);

        Assert.Single(vm.Documents);
        Assert.Same(first, vm.ActiveDocument);
    }

    [Fact]
    public void NextTab_WrapsAround()
    {
        var vm = new MainViewModel();
        vm.CreateNewDocument();
        vm.SelectedTabIndex = 1;

        vm.NextTab();

        Assert.Equal(0, vm.SelectedTabIndex);
    }

    [Fact]
    public void ToggleWordWrap_FlipsFlag()
    {
        var vm = new MainViewModel { IsWordWrapEnabled = true };
        vm.ToggleWordWrap();

        Assert.False(vm.IsWordWrapEnabled);
    }
}
