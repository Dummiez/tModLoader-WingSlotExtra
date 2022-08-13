/*using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using TerraUI.Objects;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace WingSlotExtra {
    internal class WingSlotPlayer : ModPlayer {
        private const string HiddenTag = "hidden";
        private const string WingsTag = "wings";
        private const string VanityWingsTag = "vanitywings";
        private const string WingDyeTag = "wingdye";

        public UIItemSlot EquipSlot;
        public UIItemSlot VanitySlot;
        public UIItemSlot DyeSlot;

        public override void clientClone(ModPlayer clientClone) {
            if (clientClone is not WingSlotPlayer clone)
            {
                return;
            }

            clone.EquipSlot.Item = EquipSlot.Item.Clone();
            clone.VanitySlot.Item = VanitySlot.Item.Clone();
            clone.DyeSlot.Item = DyeSlot.Item.Clone();
        }

        public override void SendClientChanges(ModPlayer clientPlayer) {
            if (clientPlayer is not WingSlotPlayer oldClone)
            {
                return;
            }

            if (oldClone.EquipSlot.Item.IsNotTheSameAs(EquipSlot.Item)) {
                SendSingleItemPacket(PacketMessageType.EquipSlot, EquipSlot.Item, -1, Player.whoAmI);
            }

            if(oldClone.VanitySlot.Item.IsNotTheSameAs(VanitySlot.Item)) {
                SendSingleItemPacket(PacketMessageType.VanitySlot, VanitySlot.Item, -1, Player.whoAmI);
            }

            if(oldClone.DyeSlot.Item.IsNotTheSameAs(DyeSlot.Item)) {
                SendSingleItemPacket(PacketMessageType.DyeSlot, DyeSlot.Item, -1, Player.whoAmI);
            }
        }

        internal void SendSingleItemPacket(PacketMessageType message, Item item, int toWho, int fromWho) {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)message);
            packet.Write((byte)Player.whoAmI);
            ItemIO.Send(item, packet);
            packet.Send(toWho, fromWho);
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)PacketMessageType.All);
            packet.Write((byte)Player.whoAmI);
            ItemIO.Send(EquipSlot.Item, packet);
            ItemIO.Send(VanitySlot.Item, packet);
            ItemIO.Send(DyeSlot.Item, packet);
            packet.Send(toWho, fromWho);
        }

        /// <summary>
        /// Initialize the ModPlayer.
        /// </summary>
        public override void Initialize() {
            EquipSlot = new UIItemSlot(Vector2.Zero, context: ItemSlot.Context.EquipAccessory, hoverText: "Wings",
                conditions: Slot_Conditions, drawBackground: Slot_DrawBackground, scaleToInventory: true);
            VanitySlot = new UIItemSlot(Vector2.Zero, context: ItemSlot.Context.EquipAccessoryVanity, hoverText:
                Language.GetTextValue("LegacyInterface.11") + " Wings",
                conditions: Slot_Conditions, drawBackground: Slot_DrawBackground, scaleToInventory: true);
            DyeSlot = new UIDyeItemSlot(Vector2.Zero, context: ItemSlot.Context.EquipDye, conditions: WingDyeSlot_Conditions,
                drawBackground: WingDyeSlot_DrawBackground, scaleToInventory: true);
            VanitySlot.Partner = EquipSlot;
            EquipSlot.BackOpacity = VanitySlot.BackOpacity = DyeSlot.BackOpacity = .8f;

            InitializeWings();
        }
        
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (DyeSlot.Item.stack > 0 && (EquipSlot.Item.wingSlot > 0 || VanitySlot.Item.wingSlot > 0))
            {
                drawInfo.cWings = DyeSlot.Item.dye;
            }
        }

        /// <summary>
        /// Update player with the equipped wings.
        /// </summary>
        public override void UpdateEquips() {//(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff) {
            Item wings = EquipSlot.Item;
            Item vanityWings = VanitySlot.Item;
            if(wings.stack > 0) {
                //Player.VanillaUpdateAccessory(Player.whoAmI, wings, !EquipSlot.ItemVisible, ref wallSpeedBuff, ref tileSpeedBuff, ref tileRangeBuff);
                Player.ApplyEquipFunctional(wings, EquipSlot.ItemVisible);
                Player.VanillaUpdateEquip(wings);
                if (!Player.ItemIsVisuallyIncompatible(wings))
                    Player.UpdateVisibleAccessory(EquipSlot.Item.wingSlot, wings, false);
            }

            if(vanityWings.stack > 0) {
                //Player.VanillaUpdateVanityAccessory(vanityWings);
                Player.ApplyEquipVanity(vanityWings);
                if (!Player.ItemIsVisuallyIncompatible(vanityWings))
                    Player.UpdateVisibleAccessory(VanitySlot.Item.wingSlot, vanityWings, false);
            }
        }

        /// <summary>
        /// Since there is no tModLoader hook in UpdateDyes, we use PreUpdateBuffs which is right after that.
        /// </summary>
        public override void PreUpdateBuffs() {
            // A little redundant code, but mirrors vanilla code exactly.
            if(DyeSlot.Item != null && !EquipSlot.Item.IsAir && EquipSlot.ItemVisible && EquipSlot.Item.wingSlot > 0) {
                if(EquipSlot.Item.wingSlot > 0)
                    Player.cWings = DyeSlot.Item.dye;
            }
            if(DyeSlot.Item != null && !VanitySlot.Item.IsAir) {
                if(VanitySlot.Item.wingSlot > 0)
                    Player.cWings = DyeSlot.Item.dye;
            }
        }

        /// <summary>
        /// Save the mod settings.
        /// </summary>
        public override void SaveData(TagCompound tag) {
            tag.Set(HiddenTag, EquipSlot.ItemVisible);
            tag.Set(WingsTag, ItemIO.Save(EquipSlot.Item));
            tag.Set(VanityWingsTag, ItemIO.Save(VanitySlot.Item));
            tag.Set(WingDyeTag, ItemIO.Save(DyeSlot.Item));
            //return new TagCompound {
            //    { HiddenTag, EquipSlot.ItemVisible },
            //    { WingsTag, ItemIO.Save(EquipSlot.Item) },
            //    { VanityWingsTag, ItemIO.Save(VanitySlot.Item) },
            //    { WingDyeTag, ItemIO.Save(DyeSlot.Item) }
            //};
        }

        /// <summary>
        /// Load the mod settings.
        /// </summary>
        public override void LoadData(TagCompound tag)
        { 
            SetWings(false, ItemIO.Load(tag.GetCompound(WingsTag)));
            SetWings(true, ItemIO.Load(tag.GetCompound(VanityWingsTag)));
            SetDye(ItemIO.Load(tag.GetCompound(WingDyeTag)));
            EquipSlot.ItemVisible = tag.GetBool(HiddenTag);
        }

        /// <summary>
        /// Draw the wing slot backgrounds.
        /// </summary>
        private void Slot_DrawBackground(UIObject sender, SpriteBatch spriteBatch) {
            UIItemSlot slot = (UIItemSlot)sender;

            if(ShouldDrawSlots()) {
                slot.OnDrawBackground(spriteBatch);

                if(slot.Item.stack == 0) {
                    Texture2D tex =  ModContent.Request<Texture2D>(WingSlotExtra.WingSlotBackTex).Value;  //TextureAssets.Item[ItemID.CreativeWings].Value;
                    Vector2 origin = tex.Size() / 2f * Main.inventoryScale;
                    Vector2 position = slot.Rectangle.TopLeft();

                    spriteBatch.Draw(
                        tex,
                        position + (slot.Rectangle.Size() / 2f) - (origin / 2f),
                        null,
                        Color.White * 0.35f,
                        0f,
                        origin,
                        Main.inventoryScale,
                        SpriteEffects.None,
                        0f); // layer depth 0 = front
                }
            }
        }

        /// <summary>
        /// Control what can be placed in the wing slots.
        /// </summary>
        private static bool Slot_Conditions(Item item) {
            if(item.wingSlot <= 0) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Draw the wing dye slot background.
        /// </summary>
        private static void WingDyeSlot_DrawBackground(UIObject sender, SpriteBatch spriteBatch) {
            UIItemSlot slot = (UIItemSlot)sender;

            if(!ShouldDrawSlots()) {
                return;
            }

            slot.OnDrawBackground(spriteBatch);
             
            if(slot.Item.stack != 0) {
                return;
            }
            Texture2D tex = TextureAssets.Extra[54].Value; //ModContent.Request<Texture2D>(WingSlotExtra.DyeSlotBackTex).Value;

            Rectangle rectangle = tex.Frame(3, 6, 1 % 3); //.Bounds;
            rectangle.Width -= 4;
            rectangle.Height -= 4;
            Vector2 origin = rectangle.Size() / 2f * Main.inventoryScale;
            Vector2 position = slot.Rectangle.TopLeft();

            spriteBatch.Draw(
                tex,
                position + (slot.Rectangle.Size() / 2f) - (origin / 2f),
                rectangle,
                Color.White * 0.35f,
                0f,
                origin,
                Main.inventoryScale,
                SpriteEffects.None,
                0f); // layer depth 0 = front
        }

        /// <summary>
        /// Control what can be placed in the wing dye slot.
        /// </summary>
        private static bool WingDyeSlot_Conditions(Item item) {
            return item.dye > 0 && item.hairDye < 0;
        }

        /// <summary>
        /// Draw the wing slots.
        /// </summary>
        /// <param name="spriteBatch">drawing SpriteBatch</param>
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
            if (gameTime is null)
            {
                throw new ArgumentNullException(nameof(gameTime));
            }

            if (!ShouldDrawSlots()) {
                return;
            }

            int mapH = 0;
            int rX;
            int rY;
            float origScale = Main.inventoryScale;

            Main.inventoryScale = 0.85f;

            if(Main.mapEnabled) {
                if(!Main.mapFullscreen && Main.mapStyle == 1) {
                    mapH = 256;
                }
            }

            if(!WingSlotExtra.SlotsNextToAccessories) {
                if(Main.mapEnabled) {
                    if((mapH + 600) > Main.screenHeight) {
                        mapH = Main.screenHeight - 600;
                    }
                }

                rX = Main.screenWidth - 92 - (47 * 2);
                rY = mapH + 174;

                if(Main.netMode == NetmodeID.MultiplayerClient) {
                    rX -= 47;
                }
            }
            else {
                if(Main.mapEnabled) {
                    int adjustY = 600;

                    if(Main.player[Main.myPlayer].extraAccessory) {
                        adjustY = 610 + PlayerInput.UsingGamepad.ToInt() * 30;
                    }

                    if((mapH + adjustY) > Main.screenHeight) {
                        mapH = Main.screenHeight - adjustY;
                    }
                }

                int slotCount = 7 + Main.player[Main.myPlayer].GetAmountOfExtraAccessorySlotsToShow();

                if((Main.screenHeight < 900) && (slotCount >= 8)) {
                    slotCount = 7;
                }

                rX = Main.screenWidth - 92 - 14 - (47 * 3) - (int)(TextureAssets.InventoryBack.Width() * Main.inventoryScale);
                rY = (int)(mapH + 174 + 4 + slotCount * 56 * Main.inventoryScale);
            }

            EquipSlot.Position = new Vector2(rX, rY);
            VanitySlot.Position = new Vector2(rX -= 47, rY);
            DyeSlot.Position = new Vector2(rX - 47, rY);

            VanitySlot.Draw(spriteBatch);
            EquipSlot.Draw(spriteBatch);
            DyeSlot.Draw(spriteBatch);

            Main.inventoryScale = origScale;

            EquipSlot.Update();
            VanitySlot.Update();
            DyeSlot.Update();
        }

        /// <summary>
        /// Whether to draw the UIItemSlots.
        /// </summary>
        /// <returns>whether to draw the slots</returns>
        // private static bool ShouldDrawSlots(out int slotLocation) {
        private static bool ShouldDrawSlots() {
            return Main.playerInventory && ((WingSlotExtra.SlotsNextToAccessories && Main.EquipPage == 0) ||
                    (!WingSlotExtra.SlotsNextToAccessories && Main.EquipPage == 2));
        }

        /// <summary>
        /// Initialize the items in the UIItemSlots.
        /// </summary>
        private void InitializeWings() {
            EquipSlot.Item = new Item();
            VanitySlot.Item = new Item();
            DyeSlot.Item = new Item();
            EquipSlot.Item.SetDefaults(0, true); // Can remove "0, true" once 0.10.1.5 comes out.
            VanitySlot.Item.SetDefaults(0, true);
            DyeSlot.Item.SetDefaults(0, true);
        }

        /// <summary>
        /// Set the item in the specified slot.
        /// </summary>
        /// <param name="isVanity">whether to equip in the vanity slot</param>
        /// <param name="item">wings</param>
        private void SetWings(bool isVanity, Item item) {
            if(!isVanity) {
                EquipSlot.Item = item.Clone();
            }
            else {
                VanitySlot.Item = item.Clone();
            }
        }

        /// <summary>
        /// Clear the wings from the specified slot.
        /// </summary>
        /// <param name="isVanity">whether to unequip from the vanity slot</param>
        public void ClearWings(bool isVanity) {
            if(!isVanity) {
                EquipSlot.Item = new Item();
                EquipSlot.Item.SetDefaults();
            }
            else {
                VanitySlot.Item = new Item();
                VanitySlot.Item.SetDefaults();
            }
        }

        /// <summary>
        /// Set the wing dye.
        /// </summary>
        /// <param name="item">dye</param>
        private void SetDye(Item item) {
            DyeSlot.Item = item.Clone();
        }

        /// <summary>
        /// Clear the wing dye.
        /// </summary>
        public void ClearDye() {
            DyeSlot.Item = new Item();
            DyeSlot.Item.SetDefaults();
        }

        /// <summary>
        /// Equip a set of wings.
        /// </summary>
        /// <param name="isVanity">whether the wings should go in the vanity slot</param>
        /// <param name="item">wings</param>
        public void EquipWings(bool isVanity, Item item) {
            UIItemSlot slot = (isVanity ? VanitySlot : EquipSlot);
            int fromSlot = Array.FindIndex(Player.inventory, i => i == item);

            // from inv to slot
            if(fromSlot < 0) {
                return;
            }

            // item.favorited = false;
            Player.inventory[fromSlot] = slot.Item.Clone();
            SoundEngine.PlaySound(SoundID.Grab);
            Recipe.FindRecipes();
            SetWings(isVanity, item);
        }

        /// <summary>
        /// Equip a dye.
        /// </summary>
        /// <param name="item">dye to equip</param>
        public void EquipDye(Item item) {
            int fromSlot = Array.FindIndex(Player.inventory, i => i == item);

            // from inv to slot
            if(fromSlot < 0) {
                return;
            }

            // item.favorited = false;
            Player.inventory[fromSlot] = DyeSlot.Item.Clone();
            SoundEngine.PlaySound(SoundID.Grab);
            Recipe.FindRecipes();
            SetDye(item);
        }

        /// <summary>
        /// Get the set of wings that a dye should be applied to.
        /// </summary>
        /// <returns>dyed wings</returns>
        public Item GetDyedWings() {
            if(VanitySlot.Item.stack > 0) {
                return VanitySlot.Item;
            }

            if(EquipSlot.Item.stack > 0) {
                return EquipSlot.Item;
            }

            return new Item();
        }
    }
}
*/