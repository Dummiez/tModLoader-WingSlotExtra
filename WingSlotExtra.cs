using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace WingSlotExtra
{
    public class WingSlotExtra : Mod
    {
        private static WingSlotExtra instance;
        private static bool resourcePackEnabled = false;
        private static readonly WingSlotConfig wingConfig = ModContent.GetInstance<WingSlotConfig>();

        internal static WingSlotExtra Instance { get => instance; set => instance = value; }
        internal static bool ResourcePackEnabled { get => resourcePackEnabled; set => resourcePackEnabled = value; }

        public static WingSlotConfig WingConfig => wingConfig;

        public override void Load() => Instance = this;

        public override void Unload() => Instance = null;
    }

    internal class WingSlotPlayer : ModPlayer
    {
        //private const string WingsTag = "wings";
        //private const string VanityWingsTag = "vanitywings";
        //private const string WingDyeTag = "wingdye";
        //public override void SaveData(TagCompound tag)
        //{}
        //public override void LoadData(TagCompound tag) // Attempt to load legacy save data
        //{
        //    ContentInstance<WingSlotExtraSlot>.Instance.FunctionalItem = ItemIO.Load(tag.GetCompound(WingsTag));
        //    ContentInstance<WingSlotExtraSlot>.Instance.VanityItem = ItemIO.Load(tag.GetCompound(VanityWingsTag));
        //    ContentInstance<WingSlotExtraSlot>.Instance.DyeItem = ItemIO.Load(tag.GetCompound(WingDyeTag));
        //}
        internal static bool ResourcePackCheck() // Not fool-proof but probably the next best thing to check for non-default UI
        {
            try
            {
                string[] uiArr = { "UI", "Interface", "Texture", "Overhaul", "Display", "Graphic" }; // Excluded: menu, style
                foreach (var modPack in Main.AssetSourceController.ActiveResourcePackList.EnabledPacks)
                {
                    return uiArr.Any(modPack.Name.Contains) || uiArr.Any(modPack.Description.Contains);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error fetching modpacks: {0}", e);
            }
            return false;
        }

        public override void OnEnterWorld(Player player) => WingSlotExtra.ResourcePackEnabled = ResourcePackCheck();
    }

    internal class WingSlotExtraGlobalItem : GlobalItem
    {
        public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded) => 
            (item.wingSlot > 0 && slot < 20 && modded == false) ? WingSlotExtra.WingConfig.AllowAccessorySlots : base.CanEquipAccessory(item, player, slot, modded);
    }

    internal class WingSlotExtraUpdateUI : ModSystem
    {
        private static int posX;
        private static int posY;

        internal static int PosX { get => posX; set => posX = value; }
        internal static int PosY { get => posY; set => posY = value; }

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

    public class WingSlotExtraSlot : ModAccessorySlot // Wing slot mod properties
    {
        public override string Name => "WingSlotExtra";

        public override bool IsHidden()
        {
            if (Main.EquipPageSelected == 1)
                return true;

            if (Main.playerInventory && Main.mouseItem.wingSlot > 0 && !WingSlotExtra.WingConfig.SlotsNextToAccessories)
                return false;

            return Main.playerInventory && ((WingSlotExtra.WingConfig.SlotsNextToAccessories && Main.EquipPageSelected == 2) ||
                (!WingSlotExtra.WingConfig.SlotsNextToAccessories && Main.EquipPageSelected == 0));
        }

        public override Vector2? CustomLocation => WingSlotExtra.WingConfig.SlotsAlongAccessories ? base.CustomLocation : new Vector2(WingSlotExtraUpdateUI.PosX, WingSlotExtraUpdateUI.PosY);

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) => (checkItem.wingSlot > 0);

        public override bool ModifyDefaultSwapSlot(Item currItem, int accSlotToSwapTo) => (currItem.wingSlot > 0);

        public override bool IsVisibleWhenNotEnabled() => false;

        public override string FunctionalTexture => "Terraria/Images/Item_" + ItemID.CreativeWings;
        public override string FunctionalBackgroundTexture => !WingSlotExtra.ResourcePackEnabled ? "WingSlotExtra/Assets/BG_Functional" : base.FunctionalBackgroundTexture;

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

        //public override void ApplyEquipEffects() // Vanity slot currently bugged as of 25/07/2022
        //{
        //    var loader = LoaderManager.Get<AccessorySlotLoader>();
        //    for (var i = 0; i < ModSlotPlayer.SlotCount; i++)
        //    {
        //        if (loader.ModdedIsAValidEquipmentSlotForIteration(i, Player))
        //            if (loader.Get(i).Name == ModContent.GetInstance<WingSlotExtra>().Name)
        //            {
        //                var vItem = loader.Get(i).VanityItem;
        //                if (vItem.type > ItemID.None)
        //                {
        //                    Player.UpdateVisibleAccessory(i, vItem);
        //                    Player.ApplyEquipVanity(vItem);
        //                    Player.VanillaUpdateEquip(vItem);
        //                }
        //                //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Item: {loader.Get(i).VanityItem} NetMode: {Main.netMode}"), Color.White);
        //            }
        //    }
        //}
    }
}