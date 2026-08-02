// src/UrlShortner.Web/Extensions/ConfigurationExtensions.cs
namespace UrlShortner.Web.Extensions;

public static class ConfigurationExtensions
{
    public static void LoadEnvironmentVariables(this IConfigurationBuilder builder)
    {
        var envFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".env");
        if (File.Exists(envFile))
        {
            foreach (var line in File.ReadAllLines(envFile))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
        }
    }
}