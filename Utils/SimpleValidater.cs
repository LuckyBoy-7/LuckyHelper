namespace LuckyHelper.Utils;

public class SimpleValidater
{
    private SimpleLookup valid = new();
    private SimpleLookup invalid = new();

    public SimpleValidater(List<string> items)
    {
        foreach (string item in items)
        {
            if (item.StartsWith("!"))
            {
                invalid.Add(item.Substring(1));
            }
            else
            {
                valid.Add(item);
            }
        }
    }

    public bool Valid(string roomName)
    {
        if (invalid.Contains(roomName))
            return false;
        if (valid.Contains(roomName))
            return true;
        return false;
    }
}