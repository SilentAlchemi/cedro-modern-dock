namespace CedroModernDock.Tests;

using CedroModernDock.Core.Application;
using CedroModernDock.Core.Domain;
using CedroModernDock.Core.Models;
using CedroModernDock.ViewModels;

public class SettingsRemovalTest
{
    [Fact]
    public void DockService_AllowsRemovingSettingsItem()
    {
        var repository = new InMemoryDockRepository();
        var service = new DockService(repository);
        service.AddItem(new DockSettingsItemModel());
        service.AddItem(new DockProgramItemModel("Editor", @"C:\tools\editor.exe"));

        service.RemoveItem(0);

        Assert.Single(service.GetItems());
        Assert.IsType<DockProgramItemModel>(service.GetItems()[0]);
    }

    [Fact]
    public void UpdateButtonStates_AllowsRemovingSettingsItem()
    {
        var settings = CreateSettingsViewModel(out var dockService);
        dockService.AddItem(new DockSettingsItemModel());
        settings.Initialize();

        settings.SelectedItemIndex = 0;
        settings.RemoveSelected();

        Assert.Empty(dockService.GetItems());
        Assert.False(settings.CanRemove);
        Assert.True(settings.CanAddSettings);
    }

    [Fact]
    public void AddSettings_AddsGearWhenMissing()
    {
        var settings = CreateSettingsViewModel(out var dockService);
        dockService.AddItem(new DockWindowsModuleItemModel("Start Menu", "start"));
        settings.Initialize();

        Assert.True(settings.CanAddSettings);
        settings.AddSettings();

        Assert.Contains(dockService.GetItems(), i => i is DockSettingsItemModel);
        Assert.False(settings.CanAddSettings);
    }

    [Fact]
    public void AddSettings_DoesNotDuplicateExistingGear()
    {
        var settings = CreateSettingsViewModel(out var dockService);
        dockService.AddItem(new DockSettingsItemModel());
        settings.Initialize();

        Assert.False(settings.CanAddSettings);
        settings.AddSettings();

        Assert.Single(dockService.GetItems());
        Assert.IsType<DockSettingsItemModel>(dockService.GetItems()[0]);
    }

    private static SettingsViewModel CreateSettingsViewModel(out DockService dockService)
    {
        var repository = new InMemoryDockRepository();
        dockService = new DockService(repository);
        var appearance = new DockAppearanceService(dockService);
        var positioning = new DockPositioningService(
            dockService,
            new FakeScreenBoundsProvider(new ScreenBounds(0, 0, 1920, 1080)));
        var appServices = new AppServices(
            DockService: dockService,
            AppearanceService: appearance,
            PositioningService: positioning,
            ItemActionService: null!,
            WindowPreviewService: null!,
            IconGateway: null!,
            LocalizationService: new LocalizationService(dockService));

        return new SettingsViewModel(
            appServices,
            dockRefreshAction: () => { },
            positioningModeChangeAction: _ => { });
    }

    private sealed class FakeScreenBoundsProvider(ScreenBounds bounds) : IScreenBoundsProvider
    {
        public ScreenBounds GetPrimaryScreenBounds() => bounds;
    }

    private sealed class InMemoryDockRepository : IDockRepository
    {
        private readonly DockModel _model = new();
        public DockModel Load() => _model;
        public void Save(DockModel model) { }
    }
}
