// <copyright file="Home.razor.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Components.Pages;

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Contains interaction logic for the home page.
/// </summary>
public partial class Home
{
    /// <summary>
    /// The server index.
    /// </summary>
    private const int ServerIndex = 1;

    /// <summary>
    /// The type of server to download.
    /// </summary>
    private static readonly string DownloadType = OperatingSystem.IsWindows() ?
        "serverBedrockWindows" :
        "serverBedrockLinux";

    /// <summary>
    /// The endpoint for obtaining the links to download the servers.
    /// </summary>
    private static readonly Uri Endpoint = new("https://net-secondary.web.minecraft-services.net/api/v1.0/download/links");

    /// <summary>
    /// The root path for local file storage.
    /// </summary>
    private static readonly string PortalDirectory = OperatingSystem.IsWindows() ?
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GSD", "MinecraftPortal") :
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "gsd", "mcportal");

    /// <summary>
    /// The path to download the servers.
    /// </summary>
    private static readonly string ImagesDirectory = OperatingSystem.IsWindows() ?
        Path.Combine(PortalDirectory, "Images") :
        Path.Combine(PortalDirectory, "images");

    /// <summary>
    /// The path to extract the servers.
    /// </summary>
    private static readonly string ServersDirectory = OperatingSystem.IsWindows() ?
        Path.Combine(PortalDirectory, "Servers") :
        Path.Combine(PortalDirectory, "servers");

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string StatusMessage { get; set; }

    /// <summary>
    /// Gets the link to download a server.
    /// </summary>
    /// <param name="downloadType">The type of server to download.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation whose result is the link to download a server.</returns>
    public static async Task<string> GetDownloadUrlAsync(string downloadType)
    {
        using var http = new HttpClient();
        var json = await http.GetStringAsync(Endpoint).ConfigureAwait(false);
        var root = JsonSerializer.Deserialize<JsonObject>(json);
        var links = root?["result"]?["links"]?.AsArray();

        return links?.FirstOrDefault(link =>
            {
                var value = link["downloadType"]?.GetValue<string>();
                return (value != null) && value.Equals(downloadType, StringComparison.Ordinal);
            })
            ?["downloadUrl"]?.GetValue<string>();
    }

    /// <summary>
    /// Downloads the latest server.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private async Task DownloadAsync()
    {
        try
        {
            var downloadUrl = await GetDownloadUrlAsync(DownloadType).ConfigureAwait(false);

            if (downloadUrl == null)
            {
                this.StatusMessage = "Download URL not found.";
                return;
            }

            var uri = new Uri(downloadUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            var filePath = Path.Combine(ImagesDirectory, fileName);
            this.StatusMessage = $"Downloading {fileName}...";
            await this.InvokeAsync(this.StateHasChanged).ConfigureAwait(false);

            Directory.CreateDirectory(ImagesDirectory);

            using var httpClient = new HttpClient();

            using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var fsDisposable = fs.ConfigureAwait(false);

            await response.Content.CopyToAsync(fs).ConfigureAwait(false);

            this.StatusMessage = $"Download complete: {filePath}";
        }
        catch (Exception ex) when (ex is
            HttpRequestException or
            SocketException or
            OperationCanceledException or
            IOException or
            UnauthorizedAccessException or
            SecurityException or
            NotSupportedException)
        {
            this.StatusMessage = $"Download failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Extracts the minecraft server files.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private async Task ExtractAsync()
    {
        try
        {
            var imagePath = Directory.GetFiles(ImagesDirectory, "*.zip").FirstOrDefault();

            if (imagePath == null)
            {
                this.StatusMessage = "No image found.";
                return;
            }

            var serverDirectory = Path.Combine(ServersDirectory, ServerIndex.ToString(CultureInfo.InvariantCulture));
            var fileName = Path.GetFileName(imagePath);
            this.StatusMessage = $"Extracting {fileName} to {serverDirectory}...";
            await this.InvokeAsync(this.StateHasChanged).ConfigureAwait(false);

            if (Directory.Exists(serverDirectory))
            {
                Directory.Delete(serverDirectory, true);
            }

            Directory.CreateDirectory(serverDirectory);

            await Task.Run(() => ExtractFilesAsync(imagePath, serverDirectory)).ConfigureAwait(false);

            this.StatusMessage = $"Extracted {fileName} to {serverDirectory}";
        }
        catch (Exception ex) when (ex is
            IOException or
            UnauthorizedAccessException or
            SecurityException or
            InvalidDataException or
            NotSupportedException)
        {
            this.StatusMessage = $"Extraction failed: {ex.Message}";
        }

        return;

        async Task ExtractFilesAsync(string imagePath, string serverDirectory)
        {
            var sw = Stopwatch.StartNew();

            using var archive = ZipFile.OpenRead(imagePath);
            var total = archive.Entries.Count;
            var processed = 0;

            foreach (var entry in archive.Entries)
            {
                var destinationPath = Path.Combine(serverDirectory, entry.FullName);

                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(destinationPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                    entry.ExtractToFile(destinationPath, true);
                }

                processed++;

                // Update status every N files or 500ms
                if ((processed % 10 == 0) || (sw.ElapsedMilliseconds > 500))
                {
                    this.StatusMessage = $"Extracting {processed}/{total} files...";
                    await this.InvokeAsync(this.StateHasChanged).ConfigureAwait(false);
                    sw.Restart();
                }
            }
        }
    }

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private async Task StartServerAsync()
    {
        var serverDirectory = Path.Combine(ServersDirectory, ServerIndex.ToString(CultureInfo.InvariantCulture));
        var serverExecutable = Path.Combine(serverDirectory, OperatingSystem.IsWindows() ? "bedrock_server.exe" : "bedrock_server");

        if (!File.Exists(serverExecutable))
        {
            this.StatusMessage = "Server executable not found.";
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = serverExecutable,
                WorkingDirectory = serverDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (!OperatingSystem.IsWindows())
            {
                // Required for Bedrock Linux server
                psi.Environment["LD_LIBRARY_PATH"] = serverDirectory;
            }

            var log = new StringBuilder();

            using var process = new Process();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    log.AppendLine(e.Data);
                    this.StatusMessage = log.ToString();
                    _ = this.InvokeAsync(this.StateHasChanged).ConfigureAwait(false);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    log.AppendLine(e.Data);
                    this.StatusMessage = log.ToString();
                    _ = this.InvokeAsync(this.StateHasChanged).ConfigureAwait(false);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            this.StatusMessage = "Minecraft server started.";

            await process.WaitForExitAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is
            InvalidOperationException or
            Win32Exception or
            OperationCanceledException or
            PlatformNotSupportedException)
        {
            this.StatusMessage = $"Failed to start server: {ex.Message}";
        }
    }
}