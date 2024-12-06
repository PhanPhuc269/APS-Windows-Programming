using App.Helpers;

using Windows.UI.ViewManagement;
using System.Diagnostics;
using App.Services;

namespace App;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;
    private readonly UISettings settings;

    public MainWindow()
    {
        InitializeComponent();
        NgrokService.StartNgrok();
        InitializeNgrokUrlAsync();


        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
    }

    private async Task InitializeNgrokUrlAsync()
    {
        var ngrokUrl = await NgrokHelper.GetNgrokUrlAsync();
        if (!string.IsNullOrEmpty(ngrokUrl))
        {
            var notifyUrl = $"{ngrokUrl}/callback/";
            var returnUrl = $"{ngrokUrl}/return/";

            Console.WriteLine($"Updated Momo Notify URL: {notifyUrl}");

            // Sủa dòng cấu hình trong file .env
            var envFilePath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\App\.env");
            if (File.Exists(envFilePath))
            {
                var lines = await File.ReadAllLinesAsync(envFilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("MOMO_ENDPOINT_URL="))
                    {
                        lines[i] = $"MOMO_ENDPOINT_URL={notifyUrl}";
                        break;
                    }
                }
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("MOMO_ENDPOINT_NOTICEURL="))
                    {
                        lines[i] = $"MOMO_ENDPOINT_NOTICEURL={returnUrl}";
                        break;
                    }
                }
                await File.WriteAllLinesAsync(envFilePath, lines);
            }

        }
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}

