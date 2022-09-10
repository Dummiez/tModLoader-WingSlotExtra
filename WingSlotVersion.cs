using Microsoft.Xna.Framework;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WingSlotExtra
{
    internal class WingSlotExtraVersion : ModSystem
    {
        static readonly HttpClient client = new();
        static readonly string url = "https://raw.githubusercontent.com/Dummiez/tModLoader-WingSlotExtra/main/build.txt";
        public override void OnWorldLoad() => CheckLatestVersion();
        public static void CheckLatestVersion(bool worldLoaded = false)
        {
            if (!NetworkInterface.GetIsNetworkAvailable() || Main.netMode == NetmodeID.SinglePlayer && !worldLoaded)
                return;
            Task.Run(async () => {
                while (Main.gameMenu)
                    Thread.Sleep(new TimeSpan(0, 0, 1));
                try
                {
                    HttpResponseMessage response;
                    response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var fetchVersion = content.Replace(" ", "");
                        if (fetchVersion.Contains("version="))
                        {
                            fetchVersion = fetchVersion.Split(new[] { "version=" }, StringSplitOptions.None)[1].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)[0];
                            var LatestVersion = new Version(fetchVersion);
                            if (LatestVersion > WingSlotExtra.Instance.Version)
                            {
                                if (worldLoaded)
                                {
                                    Main.NewText($"A new version of {WingSlotExtra.Instance.Name} ({WingSlotExtra.Instance.DisplayName}) is available: v{LatestVersion}", Color.LightBlue);
                                    if (content.Contains("patchNotes = "))
                                    {
                                        string patchNote = content.Split(new[] { "patchNotes = " }, StringSplitOptions.None)[1].Split(new[] { "\".*?\"" }, StringSplitOptions.None)[0].Replace("\"", "").Trim('\r', '\n').Replace("\\n", "\r\n");
                                        Main.NewText(patchNote, Color.LightBlue);
                                    }
                                }
                                else Console.WriteLine($"[{WingSlotExtra.Instance.Name}] A new version is available: v{LatestVersion}");
                            }
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"[{WingSlotExtra.Instance.Name}] {e}");
                }
            });
        }
    }
    internal class WingSlotExtraSystem : ModPlayer
    {
        public override void OnEnterWorld(Player player) => WingSlotExtraVersion.CheckLatestVersion(true);
    }
}
