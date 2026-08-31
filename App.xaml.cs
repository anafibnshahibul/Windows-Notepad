using System.Windows;

namespace AdvancedNotepad.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Sync Mica/theme with Windows system setting on launch
        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
            Wpf.Ui.Appearance.ApplicationTheme.Unknown, // Unknown = follow system
            Wpf.Ui.Controls.WindowBackdropType.Mica,
            updateAccent: true);
    }
}