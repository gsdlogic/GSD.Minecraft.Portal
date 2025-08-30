// <copyright file="ErrorPage.razor.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Components.Pages;

using System.Diagnostics;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Contains interaction logic for the error page.
/// </summary>
public partial class ErrorPage
{
    /// <summary>
    /// Gets or sets the HTTP context.
    /// </summary>
    [CascadingParameter]
    private HttpContext HttpContext { get; set; }

    /// <summary>
    /// Gets or sets the request ID.
    /// </summary>
    private string RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether to show the request ID.
    /// </summary>
    private bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);

    /// <summary>
    /// Method invoked when the component is ready to start, having received its
    /// initial parameters from its parent in the render tree.
    /// </summary>
    protected override void OnInitialized()
    {
        this.RequestId = Activity.Current?.Id ?? this.HttpContext?.TraceIdentifier;
    }
}