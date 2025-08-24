// <copyright file="Home.razor.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Components.Pages;

using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Contains interaction logic for the home page.
/// </summary>
public partial class Home
{
    /// <summary>
    /// The endpoint for obtaining the links to download the servers.
    /// </summary>
    private static readonly Uri Endpoint = new("https://net-secondary.web.minecraft-services.net/api/v1.0/download/links");

    /// <summary>
    /// The path to download the servers.
    /// </summary>
    private static readonly string ServersDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GSD.Minecraft.Portal", "servers");

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
        var downloadUrl = await GetDownloadUrlAsync("serverBedrockLinux").ConfigureAwait(false);

        if (downloadUrl == null)
        {
            this.StatusMessage = "Download URL not found.";
            return;
        }

        try
        {
            var uri = new Uri(downloadUrl);
            var fileName = Path.GetFileName(uri.LocalPath);

            using var httpClient = new HttpClient();

            Directory.CreateDirectory(ServersDirectory);
            var filePath = Path.Combine(ServersDirectory, fileName);

            this.StatusMessage = $"Downloading {fileName}...";

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
            OperationCanceledException)
        {
            this.StatusMessage = $"Download failed: {ex.Message}";
        }
    }
}