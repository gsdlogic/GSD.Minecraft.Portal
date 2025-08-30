// <copyright file="ServerPropertiesEditor.cs" company="GSD Logic">
//   Copyright © 2025 GSD Logic. All Rights Reserved.
// </copyright>

namespace GSD.Minecraft.Portal.Services;

/// <summary>
/// Provides an editor for the server properties file.
/// </summary>
public class ServerPropertiesEditor
{
    /// <summary>
    /// The path to the properties file.
    /// </summary>
    private readonly string filePath;

    /// <summary>
    /// The dictionary containing the server properties.
    /// </summary>
    private readonly Dictionary<string, string> properties = new();

    /// <summary>
    /// The raw lines to preserve comments and formatting.
    /// </summary>
    private readonly List<string> rawLines = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerPropertiesEditor" /> class.
    /// </summary>
    /// <param name="filePath">The path to the properties file.</param>
    public ServerPropertiesEditor(string filePath)
    {
        this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        this.Load();
    }

    /// <summary>
    /// Gets the collection of keys.
    /// </summary>
    public IEnumerable<string> Keys => this.properties.Keys;

    /// <summary>
    /// Gets the value for the specified key.
    /// </summary>
    /// <param name="key">The key whose value will be returned.</param>
    /// <returns>The value for the specified key.</returns>
    public string Get(string key)
    {
        return this.properties.GetValueOrDefault(key);
    }

    /// <summary>
    /// Saves the properties file.
    /// </summary>
    public void Save()
    {
        File.WriteAllLines(this.filePath, this.rawLines);
    }

    /// <summary>
    /// Gets the value for the specified key.
    /// </summary>
    /// <param name="key">The key whose value will be set.</param>
    /// <param name="value">The value to set.</param>
    public void Set(string key, string value)
    {
        this.properties[key] = value;

        for (var i = 0; i < this.rawLines.Count; i++)
        {
            if (this.rawLines[i].StartsWith(key + "=", StringComparison.Ordinal))
            {
                this.rawLines[i] = $"{key}={value}";
                return;
            }
        }

        this.rawLines.Add($"{key}={value}");
    }

    /// <summary>
    /// Loads the properties from the file.
    /// </summary>
    private void Load()
    {
        this.rawLines.Clear();
        this.properties.Clear();

        foreach (var line in File.ReadAllLines(this.filePath))
        {
            this.rawLines.Add(line);

            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split('=', 2);

            if (parts.Length == 2)
            {
                this.properties[parts[0].Trim()] = parts[1].Trim();
            }
        }
    }
}