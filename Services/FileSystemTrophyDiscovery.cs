using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TrophiesDisplay.Models;

namespace TrophiesDisplay.Services
{
    public class FileSystemTrophyDiscovery : ITrophyDiscovery
    {
        private readonly string _trophiesPath;

        public FileSystemTrophyDiscovery()
        {
            _trophiesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/data");
        }

        public IEnumerable<TrophyMetadata> DiscoverTrophies()
        {
            if (!Directory.Exists(_trophiesPath))
                return Enumerable.Empty<TrophyMetadata>();

            return Directory.GetDirectories(_trophiesPath)
                .Select(dir =>
                {
                    var metadataPath = Path.Combine(dir, "metadata.json");
                    if (!File.Exists(metadataPath)) return null;

                    var json = File.ReadAllText(metadataPath);

                    try
                    {
                        // Deserialize the JSON into a TrophyMetadata object
                        var metadata = JsonSerializer.Deserialize<TrophyMetadata>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (metadata != null)
                        {
                            metadata.ImageUrl = ResolvePath(dir, metadata.ImageUrl);
                            metadata.WinnerImage = ResolvePath(dir, metadata.WinnerImage);
                        }

                        return metadata;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Invalid metadata.json in {dir}: {ex.Message}");
                        return null;
                    }
                })
                .Where(metadata => metadata != null)!;
        }

        private string? ResolvePath(string directory, string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;

            return Path.Combine("/data", Path.GetFileName(directory), Path.GetFileName(relativePath)).Replace("\\", "/");
        }
    }
}
