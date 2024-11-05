using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Utils;

public static class InputUtils
{
    public static void AddInputKey(Keys key)
    {
        var tmp = MInput.Keyboard.CurrentState.GetPressedKeys().ToList();
        tmp.Add(key);
        MInput.Keyboard.CurrentState = new KeyboardState(tmp.ToArray());
    }
}