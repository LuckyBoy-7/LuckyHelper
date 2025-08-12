using System.Text;
using Celeste.Mod.Entities;
using Lucky.Kits.Extensions;
using LuckyHelper.Extensions;
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
    private const string DebugPostfix = "(LuckyHelper/LogicFlagTriggerDebug)";
    private FlagLogicOperator flagLogicJudge;

    public LogicFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        string exp = data.Attr("conditionFlagExpression");
        debug = data.Bool("debug");

        if (!CheckValidBracket(exp))
        {
            TryAddWarningData("Invalid Brackets");
            return;
        }


        HashSet<char> allOperators = FlagLogicOperator.AllOperators;
        List<char> brackets = new() { '(', ')' };
        List<string> elements = Cleanup(exp, allOperators, brackets);
        var res = ParseElements(elements, allOperators, brackets);

        GetRootLogicOperatorByParsedElements(res, allOperators);
    }

    private void GetRootLogicOperatorByParsedElements(List<string> res, HashSet<char> allOperators)
    {
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

        flagLogicJudge = logics.Count > 0 ? logics[0] : new AlwaysTrueFlagLogicOperator();
    }

    private List<string> ParseElements(List<string> elements, HashSet<char> allOperators, List<char> brackets)
    {
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
#if DEBUG
        foreach (string s in res)
        {
            LogUtils.LogWarning(s);
        }

        LogUtils.LogWarning("=====================");
#endif
        if (debug)
            debugInfo.Add($"[{string.Join(" ", res)}](Parsed Expression)");

        return res;
    }

    private List<string> Cleanup(string exp, HashSet<char> allOperators, List<char> brackets)
    {
        List<string> elements = new();
        StringBuilder sb = new();
        // 预处理, 把操作符都换成一个(clean up)
        exp = exp.Replace(" ", ""); // 去除空格
        exp = exp.Replace("!!", ""); // 去除双重否定( 
        int n = exp.Length;
        int i = 0;
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

                elements.Add(start.ToString());
            }
            else
            {
                while (i < n && !allOperators.Contains(exp[i]) && !brackets.Contains(exp[i]))
                {
                    sb.Append(exp[i++]);
                }

                string flag = sb.ToString().Trim();
                sb.Clear();
                elements.Add(flag);
            }
        }

        if (debug)
        {
            debugInfo.Add($"[{string.Join("", elements)}](Cleaned Expression)");
        }

#if DEBUG
        foreach (string element in elements)
        {
            LogUtils.LogWarning(element);
        }

        LogUtils.LogWarning("=====================");
#endif
        return elements;
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
            Session session = this.Session();
            List<string> toRemove = new();
            foreach (var sessionFlag in session.Flags)
            {
                if (sessionFlag.EndsWith(DebugPostfix))
                    toRemove.Add(sessionFlag);
            }

            toRemove.ForEach(flag => session.SetFlag(flag, false));
            foreach (var parsedConditionFlag in debugInfo)
            {
                session.SetFlag(parsedConditionFlag + DebugPostfix);
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
        if (flagLogicJudge == null)
        {
            session.SetFlag(flag, false);
            return false;
        }

        bool canSet = flagLogicJudge.GetResult(session);
        session.SetFlag(flag, canSet);
        return canSet;
    }
}