// IDrawable.cs
public interface IDrawable
{
    string DisplayName { get; }
    UnityEngine.Sprite Icon { get; }
    int RarityHelper { get; }
}