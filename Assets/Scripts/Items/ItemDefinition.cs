using System;
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
    [Tooltip("Arraste o script (.cs) da habilidade. Ao desbloquear, um componente desse tipo " +
             "será adicionado diretamente ao player — não é necessário nenhum objeto/prefab de exemplo.")]
    [SerializeField] string abilityTypeName;

    [Tooltip("Nome da habilidade exibido no tooltip. Se vazio, usa o nome do item.")]
    public string abilityName = "";

    [TextArea]
    [Tooltip("O que a habilidade faz, exibido no bloco 'Habilidade Especial' do tooltip.")]
    public string abilityDescription = "";

    public string DisplayName => itemName;
    public Sprite Icon => icon;
    public int CurrentRarity => rarity;

    public Type GetAbilityType() =>
        string.IsNullOrEmpty(abilityTypeName) ? null : Type.GetType(abilityTypeName);
}