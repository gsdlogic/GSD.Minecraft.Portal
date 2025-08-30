// <copyright file="ServerManagerSettings.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Services;

/// <summary>
/// Provides settings for the server manager.
/// </summary>
public class ServerManagerSettings
{
    /// <summary>
    /// The root path for local file storage.
    /// </summary>
    private static readonly string PortalDirectory = OperatingSystem.IsWindows() ?
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GSD", "MinecraftPortal") :
        "/opt/mcportal";

    /// <summary>
    /// Gets or sets the type of server to download.
    /// </summary>
    public string DownloadType { get; set; } = OperatingSystem.IsWindows() ?
        "serverBedrockWindows" :
        "serverBedrockLinux";

    /// <summary>
    /// Gets or sets the endpoint for obtaining the links to download the servers.
    /// </summary>
    public Uri Endpoint { get; set; } = new("https://net-secondary.web.minecraft-services.net/api/v1.0/download/links");

    /// <summary>
    /// Gets or sets the path to download the servers.
    /// </summary>
    public string ImagesDirectory { get; set; } = OperatingSystem.IsWindows() ?
        Path.Combine(PortalDirectory, "Images") :
        Path.Combine(PortalDirectory, "images");

    /// <summary>
    /// Gets or sets the path to extract the servers.
    /// </summary>
    public string ServerDirectory { get; set; } = OperatingSystem.IsWindows() ?
        Path.Combine(PortalDirectory, "Server") :
        Path.Combine(PortalDirectory, "server");
}