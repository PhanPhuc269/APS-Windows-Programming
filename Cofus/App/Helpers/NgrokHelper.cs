using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Helpers;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class NgrokHelper
{
    private const string ApiUrl = "http://127.0.0.1:4040/api/tunnels";

    public static async Task<string> GetNgrokUrlAsync()
    {
        using HttpClient client = new HttpClient();
        var response = await client.GetAsync(ApiUrl);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        var tunnels = doc.RootElement.GetProperty("tunnels");
        foreach (var tunnel in tunnels.EnumerateArray())
        {
            var publicUrl = tunnel.GetProperty("public_url").GetString();
            if (publicUrl?.StartsWith("https") == true)
            {
                return publicUrl;
            }
        }

        return string.Empty;
    }
}
