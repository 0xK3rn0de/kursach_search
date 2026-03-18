using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Data.Serialization
{
    public class IndexSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task SaveAsync(InvertedIndex index, string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, index, Options);
        }

        public async Task<InvertedIndex?> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<InvertedIndex>(stream, Options);
        }
    }
}
