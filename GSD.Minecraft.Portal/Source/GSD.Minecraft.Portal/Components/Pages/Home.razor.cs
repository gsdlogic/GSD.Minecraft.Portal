// <copyright file="Home.razor.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Components.Pages;

public partial class Home
{
    public string DownloadUrl { get; set; }

    public string StatusMessage { get; set; }

    protected override void OnInitialized()
    {
        var paths = Directory.GetDirectories("/");
        this.StatusMessage = string.Join(" ", paths);
    }

    private async Task DownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(this.DownloadUrl))
        {
            this.StatusMessage = "Please enter a valid URL.";
            return;
        }

        try
        {
            var uri = new Uri(this.DownloadUrl);
            var fileName = Path.GetFileName(uri.LocalPath);

            using var httpClient = new HttpClient();

            var targetFolder = "/srv/mcportal/servers";
            Directory.CreateDirectory(targetFolder);
            var filePath = Path.Combine(targetFolder, fileName);

            this.StatusMessage = $"Downloading {fileName}...";

            using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var fsDisposable = fs.ConfigureAwait(false);

            await response.Content.CopyToAsync(fs).ConfigureAwait(false);

            this.StatusMessage = $"Download complete: {filePath}";
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Download failed: {ex.Message}";
        }
    }
}