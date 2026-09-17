using System;

public class StatDescriptor
{
    public EStatKey Key;
    public string Label;
    public string Group;
    public bool Highlight;
    public Func<string> GetValue;
}
