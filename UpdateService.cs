using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace LightBarForKeyboard
{
    public class UpdateInfo
    {
        public Version Version { get; set; } = new Version(1, 0, 0);
        public string TagName { get; set; } = string.Empty;
        public string ReleaseUrl { get; set; } = string.Empty;
        public string? InstallerUrl { get; set; }
        public string? InstallerFileName { get; set; }
        public long InstallerSize { get; set; }
        public string ReleaseNotes { get; set; } = string.Empty;
    }

    public static class UpdateService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        static UpdateService()
        {
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LightBarForKeyboard-Updater");
        }

        public static async Task<UpdateInfo?> CheckForUpdatesAsync(Version currentVersion)
        {
            try
            {
                const string apiUrl = "https://api.github.com/repos/MediaExpres/LightBarForKeyboard/releases/latest";
                string json = await HttpClient.GetStringAsync(apiUrl);

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("tag_name", out JsonElement tagElement))
                {
                    return null;
                }

                string tag = tagElement.GetString() ?? string.Empty;
                string cleanTag = tag.TrimStart('v', 'V');

                if (!Version.TryParse(cleanTag, out Version? latestVersion))
                {
                    return null;
                }

                Version normalizedCurrent = new Version(
                    currentVersion.Major,
                    currentVersion.Minor,
                    Math.Max(0, currentVersion.Build)
                );

                Version normalizedLatest = new Version(
                    latestVersion.Major,
                    latestVersion.Minor,
                    Math.Max(0, latestVersion.Build)
                );

                if (normalizedLatest <= normalizedCurrent)
                {
                    return null;
                }

                string htmlUrl = root.TryGetProperty("html_url", out JsonElement urlElement)
                    ? urlElement.GetString() ?? "https://github.com/MediaExpres/LightBarForKeyboard/releases"
                    : "https://github.com/MediaExpres/LightBarForKeyboard/releases";

                string body = root.TryGetProperty("body", out JsonElement bodyElement)
                    ? bodyElement.GetString() ?? string.Empty
                    : string.Empty;

                string? installerUrl = null;
                string? installerFileName = null;
                long installerSize = 0;

                if (root.TryGetProperty("assets", out JsonElement assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
                {
                    // Preference 1: Setup / Installer executable
                    foreach (JsonElement asset in assetsElement.EnumerateArray())
                    {
                        string name = asset.TryGetProperty("name", out JsonElement nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
                        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && name.Contains("setup", StringComparison.OrdinalIgnoreCase))
                        {
                            installerFileName = name;
                            installerUrl = asset.TryGetProperty("browser_download_url", out JsonElement dlProp) ? dlProp.GetString() : null;
                            installerSize = asset.TryGetProperty("size", out JsonElement sizeProp) ? sizeProp.GetInt64() : 0;
                            break;
                        }
                    }

                    // Preference 2: Any executable if setup not found
                    if (installerUrl == null)
                    {
                        foreach (JsonElement asset in assetsElement.EnumerateArray())
                        {
                            string name = asset.TryGetProperty("name", out JsonElement nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
                            if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                installerFileName = name;
                                installerUrl = asset.TryGetProperty("browser_download_url", out JsonElement dlProp) ? dlProp.GetString() : null;
                                installerSize = asset.TryGetProperty("size", out JsonElement sizeProp) ? sizeProp.GetInt64() : 0;
                                break;
                            }
                        }
                    }
                }

                return new UpdateInfo
                {
                    Version = normalizedLatest,
                    TagName = tag,
                    ReleaseUrl = htmlUrl,
                    InstallerUrl = installerUrl,
                    InstallerFileName = installerFileName,
                    InstallerSize = installerSize,
                    ReleaseNotes = body
                };
            }
            catch
            {
                return null;
            }
        }

        public static async Task DownloadAndInstallAsync(UpdateInfo update, Action<double> onProgressChanged)
        {
            if (string.IsNullOrEmpty(update.InstallerUrl) || string.IsNullOrEmpty(update.InstallerFileName))
            {
                OpenInBrowser(update.ReleaseUrl);
                return;
            }

            string tempFilePath = Path.Combine(Path.GetTempPath(), update.InstallerFileName);

            using (HttpResponseMessage response = await HttpClient.GetAsync(update.InstallerUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;

                await using Stream contentStream = await response.Content.ReadAsStreamAsync();
                await using FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                byte[] buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalBytesRead += bytesRead;

                    if (totalBytes.HasValue && totalBytes.Value > 0)
                    {
                        double percentage = (double)totalBytesRead / totalBytes.Value * 100.0;
                        onProgressChanged(percentage);
                    }
                }
            }

            // Launch the installer
            Process.Start(new ProcessStartInfo(tempFilePath)
            {
                UseShellExecute = true
            });

            // Shutdown the current application so the installer can update files smoothly
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        public static void OpenInBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                // Ignore if browser cannot be launched
            }
        }
    }
}
