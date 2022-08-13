/*using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using TerraUI.Utilities;

namespace WingSlotExtra {
    internal class GlobalWingItem : GlobalItem {

        public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
        {
            if (item.wingSlot > 0)
            {
                return WingSlotExtra.AllowAccessorySlots;
            }
            return base.CanEquipAccessory(item, player, slot, modded);
        }
        public override bool CanRightClick(Item item) {
            return (item.wingSlot > 0 && !WingSlotExtra.OverrideRightClick());
        }

        public override void RightClick(Item item, Player player) {
            if(!CanRightClick(item)) {
                return;
            }

            WingSlotPlayer mp = player.GetModPlayer<WingSlotPlayer>();
            mp.EquipWings(KeyboardUtils.HeldDown(Keys.LeftShift), item);
        }
    }
}
*/