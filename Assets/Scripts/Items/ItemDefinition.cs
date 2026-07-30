using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item Definition")]
public class ItemDefinition : ScriptableObject, IDrawable
{
    public string itemName = "Novo Item";
    [TextArea] public string description = "";
    public Sprite icon;
    public int rarity;

    [Header("Shop")]
    public int price = 50;

    public EItemEffectType effectType;

    [Header("StatChange")]
    public EStatTarget statTarget;
    public float statValue = 10f;
    public bool isMultiplier = false;

    [Header("Ability")]
    [Tooltip("Arraste um componente (de um prefab, por exemplo) que tenha o script da habilidade. " +
             "Ao desbloquear, esse mesmo tipo de script será adicionado ao player.")]
    public MonoBehaviour abilityScript;

    public string DisplayName => itemName;
    public Sprite Icon => icon;
    public int CurrentRarity => rarity;
}