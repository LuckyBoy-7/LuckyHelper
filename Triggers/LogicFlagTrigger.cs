#define debug
using System.Text;
using Celeste.Mod.Entities;
using Lucky.Kits.Extensions;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;

namespace LuckyHelper.Triggers;

public abstract class FlagLogicOperator
{
    public static HashSet<char> AllOperators = new() { '|', '!', '&' }; // 主要怕有人不会用, 而且只有一位二进制的时候逻辑运算和算术运算都是符合直觉的(

    public static Dictionary<char, int>
        Priority = new()
        {
            { '!', 3 },
            { '&', 2 },
            { '|', 1 },
        };

    public abstract bool GetResult(Session session);

    public static FlagLogicOperator CreateOperator(char start)
    {
        switch (start)
        {
            case '!':
                return new NotFlagLogicOperator();
            case '|':
                return new OrFlagLogicOperator();
            case '&':
                return new AndFlagLogicOperator();
            default:
                return null;
        }
    }

    public static bool HasLessPriority(char ch1, char ch2)
    {
        return Priority[ch1] < Priority[ch2];
    }
}

// 一元操作符
public abstract class UnaryFlagLogicOperator : FlagLogicOperator
{
    public FlagLogicOperator Child;
}

// 二元操作符
public abstract class BinaryFlagLogicOperator : FlagLogicOperator
{
    public FlagLogicOperator Left;
    public FlagLogicOperator Right;
}

public class StringFlagLogicOperator : FlagLogicOperator
{
    private string flag;

    public StringFlagLogicOperator(string flag)
    {
        this.flag = flag;
    }

    public override bool GetResult(Session session)
    {
        return session.GetFlag(flag);
    }
}

public class AlwaysTrueFlagLogicOperator : FlagLogicOperator
{
    public override bool GetResult(Session session) => true;
}

public class NotFlagLogicOperator : UnaryFlagLogicOperator
{
    public override bool GetResult(Session session)
    {
        return !Child.GetResult(session);
    }
}

public class AndFlagLogicOperator : BinaryFlagLogicOperator
{
    public override bool GetResult(Session session)
    {
        return Left.GetResult(session) && Right.GetResult(session);
    }
}

public class OrFlagLogicOperator : BinaryFlagLogicOperator
{
    public override bool GetResult(Session session)
    {
        return Left.GetResult(session) || Right.GetResult(session);
    }
}

[CustomEntity("LuckyHelper/LogicFlagTrigger")]
[Tracked]
public class LogicFlagTrigger : FlagTrigger
{
    private List<string> debugInfo = new();
    private bool debug;
    private const string DebugPostfix = "(LuckyHelper_Logic_Flag_Trigger_Debug)";
    private FlagLogicOperator flagLogicJudge;
    private string flag;

    public LogicFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        string exp = data.Attr("conditionFlagExpression");
        flag = data.Attr("flag");
        debug = data.Bool("debug");

        if (!CheckValidBracket(exp))
        {
            TryAddWarningData("Invalid Brackets");
            return;
        }


        HashSet<char> allOperators = FlagLogicOperator.AllOperators;

        List<string> elements = new();
        List<char> brackets = new() { '(', ')' };
        StringBuilder sb = new();
        // 预处理, 把操作符都换成一个(clean up)
        exp = exp.Replace(" ", ""); // 去除空格
        exp = exp.Replace("!!", ""); // 去除双重否定( 
        int n = exp.Length;
        int i = 0;
        bool preIsNot = false;
        while (i < n)
        {
            char start = exp[i];
            if (brackets.Contains(start))
            {
                elements.Add(start.ToString());
                i += 1;
                continue;
            }

            if (allOperators.Contains(exp[i]))
            {
                while (i < n && start == exp[i]) // 允许 A &&& B 写法(什
                {
                    i += 1;
                }

                if (start == '!')
                {
                    if (i == n || allOperators.Contains(exp[i]) || brackets.Contains(exp[i]))
                    {
                        TryAddWarningData("Invalid Invert Operator");
                        return;
                    }

                    preIsNot = true;
                }
                else
                {
                    elements.Add(start.ToString());
                }
            }
            else
            {
                while (i < n && !allOperators.Contains(exp[i]) && !brackets.Contains(exp[i]))
                {
                    sb.Append(exp[i++]);
                }

                string flag = sb.ToString().Trim();
                sb.Clear();
                if (debug)
                {
                    debugInfo.Add(flag);
                }

                elements.Add(flag);
                if (preIsNot)
                {
                    preIsNot = false;
                    elements.Add("!");
                }
            }
        }

#if debug

        foreach (string element in elements)
        {
            LogUtils.LogWarning(element);
        }

        LogUtils.LogWarning("=====================");
#endif

        List<char> operators = new();
        List<string> res = new();


        foreach (string element in elements)
        {
            char op = element[0];
            if (!allOperators.Contains(op) && !brackets.Contains(op)) // 如果是 flag
            {
                res.Add(element);
            }
            else if (op == '(')
            {
                operators.Add(op);
            }
            else if (op == ')')
            {
                while (operators[^1] != '(')
                {
                    res.Add(operators.Pop().ToString());
                }

                operators.Pop();
            }
            else // 是操作符
            {
                while (operators.Count > 0 && operators[^1] != '(' && FlagLogicOperator.HasLessPriority(op, operators[^1]))
                {
                    res.Add(operators.Pop().ToString());
                }

                operators.Add(op);
            }
        }

        while (operators.Count > 0)
        {
            res.Add(operators.Pop().ToString());
        }
#if debug

        foreach (string s in res)
        {
            LogUtils.LogWarning(s);
        }

        LogUtils.LogWarning("=====================");
#endif

        List<FlagLogicOperator> logics = new();
        foreach (var s in res)
        {
            if (!allOperators.Contains(s[0])) // flag
            {
                logics.Add(new StringFlagLogicOperator(s));
            }
            else
            {
                FlagLogicOperator op = FlagLogicOperator.CreateOperator(s[0]);

                if (op is UnaryFlagLogicOperator unaryFlagLogicOperator)
                {
                    if (logics.Count > 0)
                    {
                        unaryFlagLogicOperator.Child = logics.Pop();
                        logics.Add(unaryFlagLogicOperator);
                    }
                    else
                    {
                        TryAddWarningData();
                        return;
                    }
                }
                else if (op is BinaryFlagLogicOperator binaryFlagLogicOperator)
                {
                    if (logics.Count > 1)
                    {
                        binaryFlagLogicOperator.Right = logics.Pop();
                        binaryFlagLogicOperator.Left = logics.Pop();
                        logics.Add(binaryFlagLogicOperator);
                    }
                    else
                    {
                        TryAddWarningData();
                        return;
                    }
                }
            }
        }

        if (logics.Count > 0)
        {
            flagLogicJudge = logics[0];
        }
        else
        {
            flagLogicJudge = new AlwaysTrueFlagLogicOperator();
        }
    }

    private void TryAddWarningData(string warning = "Invalid Expression")
    {
        if (debug)
            debugInfo.Add(warning + DebugPostfix);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (debug)
        {
            foreach (var parsedConditionFlag in debugInfo)
            {
                this.Session().SetFlag(parsedConditionFlag + DebugPostfix);
            }
        }
    }

    private bool CheckValidBracket(string s)
    {
        int left = 0;
        foreach (char c in s)
        {
            if (c == '(')
                left += 1;
            else if (c == ')')
            {
                left -= 1;
                if (left < 0)
                    return false;
            }
        }

        return true;
    }

    public override bool TrySetFlag()
    {
        Session session = this.Session();
        bool canSet = flagLogicJudge.GetResult(session);
        session.SetFlag(flag, canSet);
        return canSet;
    }
}