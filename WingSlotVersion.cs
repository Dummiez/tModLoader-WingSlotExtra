using Microsoft.Xna.Framework;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace WingSlotExtra
{
    internal class WingSlotExtraVersion : ModSystem
    {
        static readonly HttpClient client = new();
        public override void OnWorldLoad() => CheckLatestVersion();
        public static void CheckLatestVersion(bool worldLoaded = false)
        {
            Task.Run(async () =>
            {
                while (Main.gameMenu)
                    Thread.Sleep(new TimeSpan(0, 0, 1));
                try
                {
                    string buildResult = await client.GetStringAsync($"https://raw.githubusercontent.com/Dummiez/tModLoader-WingSlotExtra/main/build.txt");
                    var fetchVersion = buildResult.ToLower().Replace(" ", "");
                    var LatestVersion = WingSlotExtra.Instance.Version;
                    
                    if (fetchVersion.Contains("version="))
                    {
                        fetchVersion = fetchVersion.Split(new[] { "version=" }, StringSplitOptions.None)[1].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)[0];
                        LatestVersion = new Version(fetchVersion);
                        if (LatestVersion > WingSlotExtra.Instance.Version)
                        {
                            if (worldLoaded)
                            {
                                Main.NewText($"A new version of {WingSlotExtra.Instance.Name} ({WingSlotExtra.Instance.DisplayName}) is available: v{LatestVersion}", Color.LightBlue);
                                if (buildResult.Contains("patchNotes = "))
                                {
                                    var patchNote = buildResult.Split(new[] { "patchNotes = " }, StringSplitOptions.None)[1].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)[0];
                                    Main.NewText($"{patchNote}", Color.LightBlue);
                                }
                            }
                            else Console.WriteLine($"[{WingSlotExtra.Instance.Name}] A new version is available: v{LatestVersion}");
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"[{WingSlotExtra.Instance.Name}] Error checking for latest version: {e.Message}");
                }
            });
        }
    }
    internal class WingSlotExtraSystem : ModPlayer
    {
        public override void OnEnterWorld(Player player) => WingSlotExtraVersion.CheckLatestVersion(true);
    }
}
