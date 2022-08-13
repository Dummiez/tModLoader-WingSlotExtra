/*using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Achievements;
using TerraUI;
using TerraUI.Objects;
using Terraria.Audio;
using Terraria.ID;

namespace WingSlotExtra {
    // Because I can't change TerraUI, I subclass UIItemSlot to get proper dye slot behavior.
    internal class UIDyeItemSlot : UIItemSlot {
        public UIDyeItemSlot(Vector2 position, int size = 52, int context = 0, string hoverText = "",
            UIObject parent = null, ConditionHandler conditions = null, DrawHandler drawBackground = null,
            DrawHandler drawItem = null, DrawHandler postDrawItem = null, bool drawAsNormalSlot = false,
            bool scaleToInventory = false) : base(position, size, context, hoverText, parent, conditions,
            drawBackground, drawItem, postDrawItem, drawAsNormalSlot, scaleToInventory) { }

        public override void OnLeftClick() {
            if(Main.mouseItem.stack == 1 && Main.mouseItem.dye > 0 && Item.type > ItemID.None && Item.type != Main.mouseItem.type) {
                Utils.Swap(ref item, ref Main.mouseItem);
                SoundEngine.PlaySound(7);
                if(Item.stack > 0) {
                    AchievementsHelper.HandleOnEquip(Main.LocalPlayer, Item, 12);
                }
            }
            else if(Main.mouseItem.type == ItemID.None && Item.type > ItemID.None) {
                Utils.Swap(ref item, ref Main.mouseItem);
                if(Item.type == ItemID.None || Item.stack < 1) {
                    Item = new Item();
                }
                if(Main.mouseItem.type == ItemID.None || Main.mouseItem.stack < 1) {
                    Main.mouseItem = new Item();
                }
                if(Main.mouseItem.type > ItemID.None || Item.type > ItemID.None) {
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(7);
                }
            }
            else if(Main.mouseItem.dye > 0 && Item.type == ItemID.None) {
                if(Main.mouseItem.stack == 1) {
                    Utils.Swap(ref item, ref Main.mouseItem);
                    if(Item.type == ItemID.None || Item.stack < 1) {
                        Item = new Item();
                    }
                    if(Main.mouseItem.type == ItemID.None || Main.mouseItem.stack < 1) {
                        Main.mouseItem = new Item();
                    }
                    if (Main.mouseItem.type > ItemID.None || Item.type > ItemID.None)
                    {
                        Recipe.FindRecipes();
                        SoundEngine.PlaySound(7);
                    }
                }
                else {
                    Main.mouseItem.stack--;
                    Item.SetDefaults(Main.mouseItem.type);
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(7);
                }
                if(Item.stack > 0) {
                    AchievementsHelper.HandleOnEquip(Main.LocalPlayer, Item, 12);
                }
            }
            // Item.favorited = false;
        }
    }
}
*/