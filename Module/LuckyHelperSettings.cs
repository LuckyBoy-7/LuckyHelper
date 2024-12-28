using System.Text;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Module;

public class LuckyHelperSettings : EverestModuleSettings
{
    [SettingIgnore]
    public bool IsDebugging { get; set; } = true;
    public bool EnableAutoCB { get; set; }
    // public bool EnableFallingBlockBlockFloatySpaceBlock { get; set; }
    public bool EnablePlayerFallingThroughJumpThru { get; set; }

    [DefaultButtonBinding(Buttons.A, Keys.Enter)]
    public ButtonBinding PlayerFallingThroughJumpThruButton { get; set; }
    
    [DefaultButtonBinding(Buttons.A, Keys.Space)]
    public ButtonBinding GhostTransposeButton { get; set; }
}