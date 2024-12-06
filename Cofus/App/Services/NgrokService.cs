using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services;
public class NgrokService
{
    private static readonly string NgrokPath = @"D:\phuc\C#\Project\Cofus\Cofus\App\Tools\ngrok.exe"; // Đường dẫn từ .env
    private const string NgrokPort = "5000"; // Cổng ứng dụng local đang chạy

    public static void StartNgrok()
    {
        var temp= NgrokPath;
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = NgrokPath,
                Arguments = $"http {NgrokPort} -host-header=\"localhost:{NgrokPort}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        Debug.WriteLine(output);
    }
}
