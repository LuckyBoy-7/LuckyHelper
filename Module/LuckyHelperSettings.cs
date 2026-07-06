using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Module;

public class LuckyHelperSettings : EverestModuleSettings
{
    public bool EnablePlayerFallingThroughJumpThru { get; set; }

    [DefaultButtonBinding(Buttons.A, Keys.Enter)]
    public ButtonBinding PlayerFallingThroughJumpThruButton { get; set; }

    [DefaultButtonBinding(Buttons.A, Keys.Space)]
    public ButtonBinding GhostTransposeButton { get; set; }


    public ButtonBinding LayerViewerSwitchButton { get; set; }
    public ButtonBinding LayerViewerStepForward { get; set; }
    public ButtonBinding LayerViewerStepBackward { get; set; }
    public ButtonBinding LayerViewerQuickStepForward { get; set; }
    public ButtonBinding LayerViewerQuickStepBackward { get; set; }
}