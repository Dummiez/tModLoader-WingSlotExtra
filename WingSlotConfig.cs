using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WingSlotExtra // Non-english translations are done by DeepL
{
    //[LabelKey("$Mods.WingSlotExtra.Configuration.ConfigLabel")]
    public class WingSlotConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [Header("$Mods.WingSlotExtra.Configuration.WingSlotHeader")]

        [LabelKey("$Mods.WingSlotExtra.Configuration.SlotsAtAccesories")]
        [TooltipKey("$Mods.WingSlotExtra.Configuration.SlotsAtAccesoriesTooltip")]
        [DefaultValue(true)]
        public bool SlotsNextToAccessories { get; set; }

        [LabelKey("$Mods.WingSlotExtra.Configuration.SlotsAlongAccessories")]
        [TooltipKey("$Mods.WingSlotExtra.Configuration.SlotsAlongAccessoriesTooltip")]
        [DefaultValue(true)]
        public bool SlotsAlongAccessories { get; set; }
        [LabelKey("$Mods.WingSlotExtra.Configuration.AllowAccessorySlots")]
        [TooltipKey("$Mods.WingSlotExtra.Configuration.AllowAccessorySlotsTooltip")]
        [DefaultValue(false)]
        public bool AllowAccessorySlots { get; set; }
        [LabelKey("$Mods.WingSlotExtra.Configuration.AllowMultipleWings")]
        [TooltipKey("$Mods.WingSlotExtra.Configuration.AllowMultipleWingsTooltip")]
        [DefaultValue(false)]
        public bool AllowMultipleWings { get; set; }
        [LabelKey("$Mods.WingSlotExtra.Configuration.LoadoutSupportEnabled")]
        [TooltipKey("$Mods.WingSlotExtra.Configuration.LoadoutSupportEnabledTooltip")]
        [DefaultValue(false)]
        [ReadOnly(true)]
        [ReloadRequired]
        public bool LoadoutSupportEnabled { get; }

        public override void OnChanged() // Should probably be using get set instead
        {
            if (!SlotsNextToAccessories && SlotsAlongAccessories)
                SlotsAlongAccessories = false;
        }
    }
}
