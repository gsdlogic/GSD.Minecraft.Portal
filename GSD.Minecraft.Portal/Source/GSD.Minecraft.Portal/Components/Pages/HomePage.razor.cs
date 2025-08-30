// <copyright file="HomePage.razor.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Components.Pages;

using GSD.Minecraft.Portal.Services;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Contains interaction logic for the home page.
/// </summary>
public partial class HomePage
{
    /// <summary>
    /// Gets or sets the command to send to the server.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Gets or sets the output from the server.
    /// </summary>
    public string Output { get; set; }

    /// <summary>
    /// Gets or sets the server manager.
    /// </summary>
    [Inject]
    public ServerManager ServerManager { get; set; }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string StatusMessage { get; set; }

    /// <summary>
    /// Method invoked when the component is ready to start, having received its
    /// initial parameters from its parent in the render tree.
    /// </summary>
    protected override void OnInitialized()
    {
        this.ServerManager.OutputChanged += (_, _) =>
        {
            this.Output = this.ServerManager.Output;
            _ = this.InvokeAsync(this.StateHasChanged);
        };
    }

    /// <summary>
    /// Method invoked when the component has received parameters from its parent in
    /// the render tree, and the incoming values have been assigned to properties.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (!this.RendererInfo.IsInteractive)
        {
            return;
        }

        if (!string.IsNullOrEmpty(this.ServerManager.Output))
        {
            this.Output = this.ServerManager.Output;
        }
    }

    /// <summary>
    /// Downloads the latest server.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private async Task DownloadAsync()
    {
        try
        {
            var progress = new Progress<string>(message =>
            {
                this.StatusMessage = message;
                _ = this.InvokeAsync(this.StateHasChanged);
            });

            await this.ServerManager.DownloadAsync(progress).ConfigureAwait(false);
            this.StatusMessage = "Download complete.";
        }
        catch (InvalidOperationException ex)
        {
            this.StatusMessage = ex.Message;
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
            var progress = new Progress<string>(message =>
            {
                this.StatusMessage = message;
                _ = this.InvokeAsync(this.StateHasChanged);
            });

            await this.ServerManager.ExtractAsync(progress).ConfigureAwait(false);
            this.StatusMessage = "Extracted server image.";
        }
        catch (InvalidOperationException ex)
        {
            this.StatusMessage = ex.Message;
        }
    }

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    private void SendCommand()
    {
        try
        {
            this.ServerManager.SendCommand(this.Command);
        }
        catch (InvalidOperationException ex)
        {
            this.StatusMessage = ex.Message;
        }
    }

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    /// <param name="command">The command to send.</param>
    private void SendCommand(string command)
    {
        try
        {
            this.ServerManager.SendCommand(command);
            this.StatusMessage = $"> {command}";
        }
        catch (InvalidOperationException ex)
        {
            this.StatusMessage = ex.Message;
        }
    }

    /// <summary>
    /// Starts the server.
    /// </summary>
    private void StartServer()
    {
        try
        {
            this.ServerManager.StartServer();
            this.StatusMessage = "Minecraft server started.";
        }
        catch (InvalidOperationException ex)
        {
            this.StatusMessage = ex.Message;
        }
    }
}