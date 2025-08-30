// <copyright file="ServerManager.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Services;

using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

/// <summary>
/// Manages the Minecraft server.
/// </summary>
public class ServerManager : IDisposable
{
    /// <summary>
    /// The settings for the server manager.
    /// </summary>
    private readonly ServerManagerSettings settings;

    /// <summary>
    /// The server process.
    /// </summary>
    private Process serverProcess;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerManager" /> class.
    /// </summary>
    /// <param name="options">The options to configure the settings for the server manager.</param>
    public ServerManager(IOptions<ServerManagerSettings> options)
    {
        this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ServerManager" /> class.
    /// </summary>
    ~ServerManager()
    {
        this.Dispose(false);
    }

    /// <summary>
    /// Occurs when the output has changed.
    /// </summary>
    public event EventHandler OutputChanged;

    /// <summary>
    /// Gets the server output.
    /// </summary>
    public string Output { get; private set; }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Downloads the latest server.
    /// </summary>
    /// <param name="progress">The progress handler.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    public async Task DownloadAsync(IProgress<string> progress = null)
    {
        try
        {
            var downloadUrl = await this.GetDownloadUrlAsync(this.settings.DownloadType).ConfigureAwait(false) ??
                              throw new InvalidOperationException("Download URL not found.");

            var uri = new Uri(downloadUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            var filePath = Path.Combine(this.settings.ImagesDirectory, fileName);
            progress?.Report($"Downloading {fileName}...");

            Directory.CreateDirectory(this.settings.ImagesDirectory);

            using var httpClient = new HttpClient();

            using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var fsDisposable = fs.ConfigureAwait(false);

            await response.Content.CopyToAsync(fs).ConfigureAwait(false);
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
            throw new InvalidOperationException($"Download failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns a server properties editor.
    /// </summary>
    /// <returns>A server properties editor.</returns>
    public ServerPropertiesEditor EditProperties()
    {
        var editor = new ServerPropertiesEditor(Path.Combine(this.settings.ServerDirectory, "server.properties"));
        return editor;
    }

    /// <summary>
    /// Extracts the minecraft server files.
    /// </summary>
    /// <param name="progress">The progress handler.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    public async Task ExtractAsync(IProgress<string> progress = null)
    {
        try
        {
            var imagePath = Directory.GetFiles(this.settings.ImagesDirectory, "*.zip").FirstOrDefault() ??
                            throw new InvalidOperationException("No image found.");

            var fileName = Path.GetFileName(imagePath);
            progress?.Report($"Extracting {fileName} to {this.settings.ServerDirectory}...");

            if (Directory.Exists(this.settings.ServerDirectory))
            {
                Directory.Delete(this.settings.ServerDirectory, true);
            }

            Directory.CreateDirectory(this.settings.ServerDirectory);

            await Task.Run(() => ExtractFiles(imagePath, this.settings.ServerDirectory)).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is
            IOException or
            UnauthorizedAccessException or
            SecurityException or
            InvalidDataException or
            NotSupportedException)
        {
            throw new InvalidOperationException($"Extraction failed: {ex.Message}");
        }

        return;

        void ExtractFiles(string imagePath, string serverDirectory)
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

                if ((processed % 10 == 0) || (sw.ElapsedMilliseconds > 500))
                {
                    progress?.Report($"Extracting {processed}/{total} files...");
                    sw.Restart();
                }
            }
        }
    }

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    /// <param name="command">The command to send.</param>
    public void SendCommand(string command)
    {
        if (this.serverProcess == null)
        {
            throw new InvalidOperationException("Server is not started.");
        }

        this.serverProcess.StandardInput.WriteLine(command);
        this.serverProcess.StandardInput.Flush();
    }

    /// <summary>
    /// Starts the server.
    /// </summary>
    public void StartServer()
    {
        if (this.serverProcess != null)
        {
            throw new InvalidOperationException("Server already started.");
        }

        var serverExecutable = Path.Combine(this.settings.ServerDirectory, OperatingSystem.IsWindows() ? "bedrock_server.exe" : "bedrock_server");

        if (!File.Exists(serverExecutable))
        {
            throw new InvalidOperationException("Server executable not found.");
        }

        try
        {
            foreach (var process in Process.GetProcessesByName("bedrock_server"))
            {
                if (string.Equals(process.MainModule?.FileName, serverExecutable, StringComparison.OrdinalIgnoreCase))
                {
                    process.Kill();
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = serverExecutable,
                WorkingDirectory = this.settings.ServerDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (!OperatingSystem.IsWindows())
            {
                psi.Environment["LD_LIBRARY_PATH"] = this.settings.ServerDirectory;
            }

            var log = new StringBuilder();

            this.serverProcess = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true,
            };

            this.serverProcess.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    log.AppendLine(e.Data);
                    this.Output = log.ToString();
                    this.OnOutputChanged();
                }
            };

            this.serverProcess.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    log.AppendLine(e.Data);
                    this.Output = log.ToString();
                    this.OnOutputChanged();
                }
            };

            this.serverProcess.Exited += (_, _) =>
            {
                this.serverProcess?.Dispose();
                this.serverProcess = null;
            };

            this.serverProcess.Start();
            this.serverProcess.BeginOutputReadLine();
            this.serverProcess.BeginErrorReadLine();
        }
        catch (Exception ex) when (ex is
            InvalidOperationException or
            Win32Exception or
            OperationCanceledException or
            PlatformNotSupportedException)
        {
            throw new InvalidOperationException($"Failed to start server: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">Indicates whether managed resources will be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || (this.serverProcess == null))
        {
            return;
        }

        this.serverProcess.Kill();
        this.serverProcess.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="OnOutputChanged" /> when the output has changed.
    /// </summary>
    protected virtual void OnOutputChanged()
    {
        this.OutputChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the link to download a server.
    /// </summary>
    /// <param name="downloadType">The type of server to download.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation whose result is the link to download a server.</returns>
    private async Task<string> GetDownloadUrlAsync(string downloadType)
    {
        using var http = new HttpClient();
        var json = await http.GetStringAsync(this.settings.Endpoint).ConfigureAwait(false);
        var root = JsonSerializer.Deserialize<JsonObject>(json);
        var links = root?["result"]?["links"]?.AsArray();

        return links?.FirstOrDefault(link => Match(downloadType, link))?["downloadUrl"]?.GetValue<string>();

        static bool Match(string downloadType, JsonNode link)
        {
            var value = link["downloadType"]?.GetValue<string>();
            return (value != null) && value.Equals(downloadType, StringComparison.Ordinal);
        }
    }
}