using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Microsoft.CodeAnalysis;

namespace WingSlotExtra
{
    public class WingSlotExtra : Mod
    {
        private static WingSlotExtra instance;
        private static bool resourcePackEnabled = false;
        private static readonly WingSlotConfig wingConfig = ModContent.GetInstance<WingSlotConfig>();

        internal static Dictionary<int, int> storedWingSlots = [];
        internal static Dictionary<int, int> lastPlayerLoadout = [];
        internal static Dictionary<string, Dictionary<int, (Item wings, Item vanity, Item dye, bool hidden)>> loadoutData = [];
        internal static string activePlayer;

        internal static WingSlotExtra Instance { get => instance; set => instance = value; }
        internal static bool ResourcePackEnabled { get => resourcePackEnabled; set => resourcePackEnabled = value; }
        public static WingSlotConfig WingConfig => wingConfig;
        public override void Load() => Instance = this;
        public override void Unload() => Instance = null;
    }
    internal class WingSlotPlayer : ModPlayer
    {
        public override void SaveData(TagCompound tag)
        {
            if (!string.IsNullOrEmpty(Main.ActivePlayerFileData.Path) && !string.IsNullOrEmpty(WingSlotExtra.activePlayer))
            {
                var loadoutDataList = WingSlotExtra.loadoutData.Select(kvp => new TagCompound
                {
                    {"user", kvp.Key},
                    {"loadouts", kvp.Value.Select(loadoutKvp => new TagCompound
                    {
                        {"key", loadoutKvp.Key},
                        {"wings", ItemIO.Save(loadoutKvp.Value.wings)},
                        {"vanity", ItemIO.Save(loadoutKvp.Value.vanity)},
                        {"dye", ItemIO.Save(loadoutKvp.Value.dye)},
                        {"hidden", loadoutKvp.Value.hidden}
                    }).ToList()}
                }).ToList();
                tag.Add("WingSlotExtra_Loadouts", loadoutDataList);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (Main.gameMenu && !string.IsNullOrEmpty(WingSlotExtra.activePlayer))
            {
                WingSlotExtra.activePlayer = string.Empty;
            }
            if (tag.ContainsKey("WingSlotExtra_Loadouts"))
            {
                var loadoutDataList = tag.Get<List<TagCompound>>("WingSlotExtra_Loadouts");
                WingSlotExtra.loadoutData ??= []; // new Dictionary<string, Dictionary<int, (Item wings, Item vanity, Item dye, bool hidden)>>();
                foreach (var tc in loadoutDataList)
                {
                    var user = tc.GetString("user");
                    if (!WingSlotExtra.loadoutData.ContainsKey(user) && !string.IsNullOrEmpty(user))
                    {
                        var loadouts = tc.Get<List<TagCompound>>("loadouts").ToDictionary(
                            loadoutTc => loadoutTc.GetInt("key"),
                            loadoutTc => (
                                wings: ItemIO.Load(loadoutTc.Get<TagCompound>("wings")),
                                vanity: ItemIO.Load(loadoutTc.Get<TagCompound>("vanity")),
                                dye: ItemIO.Load(loadoutTc.Get<TagCompound>("dye")),
                                hidden: loadoutTc.GetBool("hidden")
                            )
                        );
                        WingSlotExtra.loadoutData.Add(user, loadouts);
                    }
                }
            }
        }
        public override void OnEnterWorld()
        {
            var playerFile = Main.ActivePlayerFileData.Path.ToString();
            var lastIndex = playerFile.LastIndexOf('\\');
            var playerID = playerFile[(lastIndex + 1)..];
            WingSlotExtra.lastPlayerLoadout = [];
            // Iterate over the outer dictionary
            foreach (var outerKvp in WingSlotExtra.loadoutData)
            {
                if (string.Equals(playerID, outerKvp.Key))
                {
                    WingSlotExtra.activePlayer = playerID;
                }
            }
            base.OnEnterWorld();
        }
    }
    internal class WingSlotExtraGlobalItem : GlobalItem
    {
        public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded) =>
            ((item.wingSlot > 0 || WingSlotExtra.storedWingSlots.ContainsKey(item.type)) && slot < 20 && modded == false) ? WingSlotExtra.WingConfig.AllowAccessorySlots : base.CanEquipAccessory(item, player, slot, modded);

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            if (WingSlotExtra.WingConfig.AllowMultipleWings && incomingItem != null && equippedItem != incomingItem && (incomingItem.wingSlot > 0 || WingSlotExtra.storedWingSlots.ContainsKey(incomingItem.type)) && !WingSlotExtra.storedWingSlots.ContainsKey(incomingItem.type))
            {
                WingSlotExtra.storedWingSlots.Add(incomingItem.type, incomingItem.wingSlot);
                incomingItem.wingSlot = 0;
            }
            return base.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
        }

        public override void UpdateEquip(Item updateItem, Player player)
        {
            if (WingSlotExtra.storedWingSlots.ContainsKey(updateItem.type))
            {
                updateItem.wingSlot = WingSlotExtra.storedWingSlots[updateItem.type];
                WingSlotExtra.storedWingSlots.Remove(updateItem.type);
            }
            base.UpdateEquip(updateItem, player);
        }
    }

    internal class WingSlotExtraUpdateUI : ModSystem
    {
        private static int posX;
        private static int posY;

        internal static int PosX { get => posX; set => posX = value; }
        internal static int PosY { get => posY; set => posY = value; }

        //public override void Load() => IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += ItemSlotDrawColourFixPatch;

        public void ItemSlotDrawColourFixPatch(ILContext il)
        {
            if (WingSlotExtra.WingConfig.LoadoutSupportEnabled)
            {
                var ilCursor = new ILCursor(il);
                var backgroundTexture = 0;
                if (!ilCursor.TryGotoNext(MoveType.After, i => i.MatchCallvirt<AccessorySlotLoader>("GetBackgroundTexture"),
                        i => i.MatchStloc(out backgroundTexture)))
                {
                    Mod.Logger.Warn($"[{WingSlotExtra.Instance.Name}] Unable to draw AccessorySlotLoader.GetBackgroundTexture.");
                }
                ilCursor.EmitLdarg3();
                ilCursor.EmitLdarg2();
                ilCursor.Emit<WingSlotExtraUpdateUI>(OpCodes.Call, "GetColor");
                ilCursor.Emit(OpCodes.Stloc_S, (byte)8);
                ilCursor.Emit<WingSlotExtraUpdateUI>(OpCodes.Call, "GetLoader");
                ilCursor.EmitLdarg3();
                ilCursor.EmitLdarg2();
                ilCursor.Emit<WingSlotExtraUpdateUI>(OpCodes.Call, "GetTexture");
                ilCursor.EmitStloc(backgroundTexture);
            }
        }

        public static AccessorySlotLoader GetLoader()
        {
            return LoaderManager.Get<AccessorySlotLoader>();
        }

        public static Color GetColor(int slot, int context)
        {
            return GetColorByLoadout(slot, context);
        }

        public static Texture2D GetTexture(AccessorySlotLoader loader, int slot, int context)
        {
            ModAccessorySlot modAccessorySlot = loader.Get(slot);
            return context switch
            {
                -12 => ModContent.RequestIfExists(modAccessorySlot.DyeBackgroundTexture, out Asset<Texture2D> asset1)
                                        ? asset1.Value
                                        : TextureAssets.InventoryBack13.Value,
                -11 => ModContent.RequestIfExists(modAccessorySlot.VanityBackgroundTexture, out Asset<Texture2D> asset2)
                                        ? asset2.Value
                                        : TextureAssets.InventoryBack13.Value,
                -10 => ModContent.RequestIfExists(modAccessorySlot.FunctionalBackgroundTexture,
                                        out Asset<Texture2D> asset3)
                                        ? asset3.Value
                                        : TextureAssets.InventoryBack13.Value,
                _ => TextureAssets.InventoryBack13.Value,
            };
        }

        public static Color GetColorByLoadout(int slot, int context)
        {
            var _lastTimeForVisualEffectsThatLoadoutWasChanged = (double)typeof(ItemSlot)
                .GetField("_lastTimeForVisualEffectsThatLoadoutWasChanged", BindingFlags.Static | BindingFlags.NonPublic)!
                .GetValue(null)!;
            Color color1 = Color.White;
            if (TryGetSlotColor(Main.LocalPlayer.CurrentLoadoutIndex, context, out Color color2))
                color1 = color2;
            Color color3 = new(color1.ToVector4() * Main.inventoryBack.ToVector4());
            float num = Utils.Remap((float)(Main.timeForVisualEffects - _lastTimeForVisualEffectsThatLoadoutWasChanged), 0.0f, 30f, 0.5f, 0.0f);
            Color white = Color.White;
            double amount = num * num * num;
            return Color.Lerp(color3, white, (float)amount);
        }

        public static bool TryGetSlotColor(int loadoutIndex, int context, out Color color)
        {
            var LoadoutSlotColors = (Color[,])typeof(ItemSlot)
                .GetField("LoadoutSlotColors", BindingFlags.Static | BindingFlags.NonPublic)!
                .GetValue(null)!;
            color = new Color();
            if (loadoutIndex < 0 || loadoutIndex >= 3)
                return false;
            int index = -1;
            switch (context)
            {
                case 8:
                case 10:
                case -10:
                    index = 0;
                    break;

                case 9:
                case 11:
                case -11:
                    index = 1;
                    break;

                case 12:
                case -12:
                    index = 2;
                    break;
            }

            if (index == -1)
                return false;
            color = LoadoutSlotColors[loadoutIndex, index];
            return true;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) // UpdateUI(GameTime gameTime) // Render wing slot interface (no controller compatibility for mod slots yet)
        {
            if (Main.gameMenu) // Adjust location of wing slot depending on current position and setting of other interfaces
                return;

            int mapH = (Main.mapEnabled && !Main.mapFullscreen && Main.mapStyle == 1) ? 256 : 0;
            Main.inventoryScale = 0.85f;

            if (!WingSlotExtra.WingConfig.SlotsNextToAccessories && Main.EquipPageSelected == 2 && Main.mouseItem.wingSlot < 0)
            {
                mapH = (Main.mapEnabled && (mapH + 600) > Main.screenHeight) ? Main.screenHeight - 600 : mapH;
                PosX = (Main.netMode == NetmodeID.MultiplayerClient) ? (Main.screenWidth - 94 - (47 * 2)) - 47 : Main.screenWidth - 94 - (47 * 2);
                PosY = mapH + 174;
            }
            else // Default slot positioning
            {
                if (Main.mapEnabled)
                {
                    int adjustY = (Main.LocalPlayer.extraAccessory) ? 610 + PlayerInput.UsingGamepad.ToInt() * 30 : 600;
                    mapH = ((mapH + adjustY) > Main.screenHeight) ? Main.screenHeight - adjustY : mapH;
                }
                int slotCount = ((Main.screenHeight < 900) && (7 + Main.LocalPlayer.GetAmountOfExtraAccessorySlotsToShow() >= 8)) ?
                    7 : 7 + Main.LocalPlayer.GetAmountOfExtraAccessorySlotsToShow(); // Not sure if this works with modded accessory slots

                PosX = Main.screenWidth - 82 - 12 - (47 * 3) - (int)(TextureAssets.InventoryBack.Width() * Main.inventoryScale);
                PosY = (int)(mapH + 174 + 4 + slotCount * 56 * Main.inventoryScale);
            }
        }
    }

    [Autoload]
    public class WingSlotExtraSlot : ModAccessorySlot // Wing slot mod properties
    {
        public override string Name => "WingSlotExtra";

        public override bool IsHidden()
        {
            if (Main.EquipPageSelected == 1)
                return true;

            if (Main.playerInventory && (Main.mouseItem.wingSlot > 0 || WingSlotExtra.storedWingSlots.ContainsKey(Main.mouseItem.type)) && !WingSlotExtra.WingConfig.SlotsNextToAccessories)
                return false;

            return Main.playerInventory && ((WingSlotExtra.WingConfig.SlotsNextToAccessories && Main.EquipPageSelected == 2) ||
                (!WingSlotExtra.WingConfig.SlotsNextToAccessories && Main.EquipPageSelected == 0));
        }

        public override Vector2? CustomLocation => WingSlotExtra.WingConfig.SlotsAlongAccessories ? base.CustomLocation : new Vector2(WingSlotExtraUpdateUI.PosX, WingSlotExtraUpdateUI.PosY);

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) => (checkItem.wingSlot > 0 || WingSlotExtra.storedWingSlots.ContainsKey(checkItem.type));

        public override bool ModifyDefaultSwapSlot(Item currItem, int accSlotToSwapTo) => (currItem.wingSlot > 0 || WingSlotExtra.storedWingSlots.ContainsKey(currItem.type));

        public override bool IsVisibleWhenNotEnabled() => false;

        public override string FunctionalTexture => "Terraria/Images/Item_" + ItemID.CreativeWings;
        //public override string FunctionalBackgroundTexture => !WingSlotExtra.ResourcePackEnabled ? "WingSlotExtra/Assets/BG_Functional" : base.FunctionalBackgroundTexture;

        public override void OnMouseHover(AccessorySlotType context)
        {
            switch (context) // Text localization for wing slot
            {
                case AccessorySlotType.FunctionalSlot:
                    Main.hoverItemName = Language.GetTextValue("Mods.WingSlotExtra.AccessorySlot.FunctionalSlot");
                    break;

                case AccessorySlotType.VanitySlot:
                    Main.hoverItemName = Language.GetTextValue("Mods.WingSlotExtra.AccessorySlot.VanitySlot");
                    break;

                case AccessorySlotType.DyeSlot:
                    Main.hoverItemName = Language.GetTextValue("Mods.WingSlotExtra.AccessorySlot.DyeSlot");
                    break;
            }
        }
/*        public override void ApplyEquipEffects() // Simple loadout support for wing slot
        {
            var loader = LoaderManager.Get<AccessorySlotLoader>();
            var playerFile = Main.ActivePlayerFileData.Path.ToString();
            var lastIndex = playerFile.LastIndexOf('\\');
            var playerID = playerFile[(lastIndex + 1)..];
            var wingSlotIndex = Player.CurrentLoadoutIndex;
            if (!WingSlotExtra.loadoutData.ContainsKey(WingSlotExtra.activePlayer))
            {
                if (string.IsNullOrEmpty(WingSlotExtra.activePlayer))
                {
                    WingSlotExtra.activePlayer = playerID;
                }
                WingSlotExtra.loadoutData.Add(WingSlotExtra.activePlayer,
                new Dictionary<int, (Item wings, Item vanity, Item dye, bool hidden)> {
                                        { 0, (new Item(), new Item(), new Item(), false) },
                                        { 1, (new Item(), new Item(), new Item(), false) },
                                        { 2, (new Item(), new Item(), new Item(), false) }
                });
            }
            if (WingSlotExtra.WingConfig.LoadoutSupportEnabled)
            {
                for (var i = 0; i < ModSlotPlayer.SlotCount; i++)
                {
                    var slot = loader.Get(i);
                    var modReady = loader.ModdedIsItemSlotUnlockedAndUsable(i, Player);
                    var lastLoad = WingSlotExtra.lastPlayerLoadout.ContainsKey(Player.whoAmI);

                    if (modReady && slot.Name == ModContent.GetInstance<WingSlotExtra>().Name)
                        if (!lastLoad || WingSlotExtra.lastPlayerLoadout[Player.whoAmI] != wingSlotIndex)
                        {
                            if (playerID == WingSlotExtra.activePlayer && Main.LocalPlayer.CurrentLoadoutIndex == wingSlotIndex && Main.LocalPlayer.whoAmI == Player.whoAmI)
                            {
                                if (!lastLoad) WingSlotExtra.lastPlayerLoadout[Player.whoAmI] = wingSlotIndex;

                                WingSlotExtra.loadoutData[WingSlotExtra.activePlayer][WingSlotExtra.lastPlayerLoadout[Player.whoAmI]] = (slot.FunctionalItem, slot.VanityItem, slot.DyeItem, slot.HideVisuals);
                                WingSlotExtra.lastPlayerLoadout[Player.whoAmI] = wingSlotIndex;
                                var (wings, vanity, dye, hidden) = WingSlotExtra.loadoutData[WingSlotExtra.activePlayer][WingSlotExtra.lastPlayerLoadout[Player.whoAmI]];
                                slot.FunctionalItem = wings;
                                slot.VanityItem = vanity;
                                slot.DyeItem = dye;
                                slot.HideVisuals = hidden;
                                ModSlotPlayer.UpdateVisibleAccessories();
                                ModSlotPlayer.UpdateVisibleVanityAccessories();
                                ModSlotPlayer.UpdateEquips();
                            }
                        }
                        else
                            if (!string.IsNullOrEmpty(WingSlotExtra.activePlayer) && WingSlotExtra.activePlayer == playerID && Main.LocalPlayer.CurrentLoadoutIndex == wingSlotIndex && Main.LocalPlayer.whoAmI == Player.whoAmI)
                            WingSlotExtra.loadoutData[WingSlotExtra.activePlayer][wingSlotIndex] = (slot.FunctionalItem, slot.VanityItem, slot.DyeItem, slot.HideVisuals);
                }
            }
            base.ApplyEquipEffects();
        }*/
    }
}