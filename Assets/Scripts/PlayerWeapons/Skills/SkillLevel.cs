using System;

public readonly struct SkillLevel : IEquatable<SkillLevel>
{
    public static readonly SkillLevel First = new(0);

    public readonly int Index;

    SkillLevel(int index)
    {
        Index = Math.Max(0, index);
    }

    public static SkillLevel FromIndex(int index) => new(index);

    public int Number => Index + 1;
    public SkillLevel Next => new(Index + 1);

    public bool Equals(SkillLevel other) => Index == other.Index;
    public override bool Equals(object obj) => obj is SkillLevel other && Equals(other);
    public override int GetHashCode() => Index;
    public override string ToString() => Number.ToString();

    public static bool operator ==(SkillLevel a, SkillLevel b) => a.Equals(b);
    public static bool operator !=(SkillLevel a, SkillLevel b) => !a.Equals(b);
}
