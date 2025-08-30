// <copyright file="PropertiesPage.razor.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Components.Pages;

using GSD.Minecraft.Portal.Services;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Contains interaction logic for the server properties page.
/// </summary>
public partial class PropertiesPage
{
    /// <summary>
    /// Gets or sets the navigation manager.
    /// </summary>
    [Inject]
    public NavigationManager NavigationManager { get; set; }

    /// <summary>
    /// Gets or sets the server manager.
    /// </summary>
    [Inject]
    public ServerManager ServerManager { get; set; }

    /// <summary>
    /// Gets or sets the server properties editor.
    /// </summary>
    private ServerPropertiesEditor Editor { get; set; }

    /// <summary>
    /// Gets or sets the new key to add.
    /// </summary>
    private string NewKey { get; set; }

    /// <summary>
    /// Gets or sets the new value to add.
    /// </summary>
    private string NewValue { get; set; }

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

        this.Editor = this.ServerManager.EditProperties();
    }

    /// <summary>
    /// Adds a new property.
    /// </summary>
    private void AddProperty()
    {
        if (string.IsNullOrEmpty(this.NewKey) || string.IsNullOrEmpty(this.NewValue))
        {
            return;
        }

        this.Editor.Set(this.NewKey, this.NewValue);
        this.NewKey = null;
        this.NewValue = null;
    }

    /// <summary>
    /// Saves the property changes.
    /// </summary>
    private void Save()
    {
        this.Editor.Save();
        this.NavigationManager.NavigateTo("/");
    }
}