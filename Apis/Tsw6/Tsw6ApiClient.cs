using System;
using System.IO;
using System.Linq;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using GameFinder.StoreHandlers.Steam.Models.ValueTypes;
using System.Runtime.InteropServices;
using NMFS = NexusMods.Paths.FileSystem;

namespace Tsw6RealtimeWeather.Apis.Tsw6;

public class Tsw6ApiKey
{
    private const string tsw6ApiKeyFileName = "CommAPIKey.txt";
    private const uint tsw6AppId = 3656800;

    private static string apiKey = string.Empty;

    private static bool cachedApiKey = false;

    public static string Get()
    {
        if (cachedApiKey)
        {
            return apiKey;
        }

        apiKey = TryToGetApiKeyFromDocuments();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = TryToGetApiKeyFromSteam();
        }

        cachedApiKey = true;
        return apiKey;
    }

    private static string TryToGetApiKeyFromDocuments()
    {
        var prefix = "";
        bool isLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        if (isLinux) {
           prefix = Path.Join(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),
                    ".steam",
                    "steam",
                    "steamapps",
                    "compatdata",
                    tsw6AppId.ToString(),
                 "pfx",
                 "drive_c",
                 "users",
                 "steamuser",
                 "Documents");
        }
        else {
            prefix = Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments);
        }

        var searchPathForNonDevelopmentMode = Path.Join(
            prefix,
            "My Games",
            "TrainSimWorld6",
            "Saved",
            "Config",
            tsw6ApiKeyFileName);

        if (File.Exists(searchPathForNonDevelopmentMode))
        {
            return File.ReadAllText(searchPathForNonDevelopmentMode);
        }

        return string.Empty;

    }

    private static string TryToGetApiKeyFromSteam()
    {
        // Search for the Steam title instead as we are in game mode.
        var steamHandler = new SteamHandler(NMFS.Shared, OperatingSystem.IsWindows() ? WindowsRegistry.Shared : null);
        var tsw6DeveloperAppId = AppId.From(tsw6AppId);
        var tsw6Game = steamHandler.FindOneGameById(tsw6DeveloperAppId, out var errors);

        if (errors.Length > 0)
        {
            foreach (var error in errors)
            {
                Logger.LogError($"Error in finding TSW6: {error}");
            }

            return string.Empty;
        }

        if (tsw6Game == null)
        {
            Logger.LogError("Error: TSW6 is null.");
            return string.Empty;
        }

        var searchPathForDevelopmentMode = Path.Join(
            tsw6Game.Path.ToString(),
            "WindowsNoEditor",
            "TS2Prototype",
            "Saved",
            "Config",
            tsw6ApiKeyFileName);

        if (!File.Exists(searchPathForDevelopmentMode))
        {
            return string.Empty;
        }

        return File.ReadAllText(searchPathForDevelopmentMode);
    }
}
