// <copyright file="ServiceCollectionExtensions.cs" company="GSD Logic">
// Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Services;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection" /> interface.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds server management to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection so that additional calls may be chained.</returns>
    public static IServiceCollection AddServerManagement(this IServiceCollection services)
    {
        services.AddSingleton<ServerManager>();
        return services;
    }
}