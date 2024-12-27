using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;


/// <summary>
/// 摘自TASHelper, 感谢钟哥!!!
/// </summary>
public static class CILCodeHelper {
    public static void CILCodeLogger(this ILCursor ilCursor, int logCount = 999999)
    {
        int cnt = 0;
        Logger.Log(LogLevel.Warn, "TAS Helper", "---- CILCodeLogger ----");
        while (logCount > 0 && ilCursor.Next is not null) {
            string str;
            if (ilCursor.Next.Operand is ILLabel label) {
                str = $"{ilCursor.Next.Offset:x4}, {ilCursor.Next.OpCode}, {ilCursor.Next.Operand} | {label.Target.Offset:x4}, {label.Target.OpCode}, {label.Target.Operand}";
            }
            else if (ilCursor.Next.Operand is Instruction ins) {
                str = $"{ilCursor.Next.Offset:x4}, {ilCursor.Next.OpCode} | {ins.Offset:x4}, {ins.OpCode}, {ins.Operand}";
            }
            else {
                str = $"{ilCursor.Next.Offset:x4}, {ilCursor.Next.OpCode}, {ilCursor.Next.Operand}";
            }
            Logger.Log(LogLevel.Warn, "TAS Helper", str);

            logCount--;
            ilCursor.Index++;
            cnt += 1;
        }

        ilCursor.Index -= cnt;
    }
}