using UnityEngine;

public abstract class SkillVariantDefinition : ScriptableObject, IDrawable
{
    public string variantName = "Nova Variante";
    public Sprite icon;
    [TextArea] public string description = "";

    public string DisplayName => variantName;
    public Sprite Icon => icon;
}
