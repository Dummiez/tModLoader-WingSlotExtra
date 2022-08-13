using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WingSlotExtra // Non-english translations are done by DeepL
{
    [Label("$Mods.WingSlotExtra.Configuration.ConfigLabel")]
    public class WingSlotConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [Header("$Mods.WingSlotExtra.Configuration.WingSlotHeader")]

        [Label("$Mods.WingSlotExtra.Configuration.SlotsAtAccesories")]
        [Tooltip("$Mods.WingSlotExtra.Configuration.SlotsAtAccesoriesTooltip")]
        [DefaultValue(true)]
        public bool SlotsNextToAccessories { get; set; }

        [Label("$Mods.WingSlotExtra.Configuration.SlotsAlongAccessories")]
        [Tooltip("$Mods.WingSlotExtra.Configuration.SlotsAlongAccessoriesTooltip")]
        [DefaultValue(true)]
        public bool SlotsAlongAccessories { get; set; }
        [Label("$Mods.WingSlotExtra.Configuration.AllowAccessorySlots")]
        [Tooltip("$Mods.WingSlotExtra.Configuration.AllowAccessorySlotsTooltip")]
        [DefaultValue(false)]
        public bool AllowAccessorySlots { get; set; }

        public override void OnChanged() // Should probably be using get set instead
        {
            if (!SlotsNextToAccessories && SlotsAlongAccessories)
                SlotsAlongAccessories = false;
        }
    }
}
