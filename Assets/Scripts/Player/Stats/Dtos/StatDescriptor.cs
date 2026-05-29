using System;

public class StatDescriptor
{
    public EStatKey Key;
    public string Label;
    public string Group;
    public Func<string> GetValue;
}
