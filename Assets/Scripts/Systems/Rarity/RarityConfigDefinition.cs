using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Systems.Rarity
{

    [CreateAssetMenu(fileName = "RarityConfig", menuName = "Config/Rarity Config")]
    public class RarityConfig : ScriptableObject
    {
        private static RarityConfig _instance;
        public static RarityConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<RarityConfig>("RarityConfig");
                if (_instance == null)
                    Debug.LogError("[RarityConfig] Asset não encontrado em Resources/RarityConfig.");
                return _instance;
            }
        }

        public List<RarityDto> rarities = new()
    {
        new() { id = "Common",    displayName = "Comum",    color = new Color(0.75f, 0.75f, 0.75f), baseWeight = 60f, weightPerLuck = -0.30f },
        new() { id = "Uncommon",  displayName = "Incomum",  color = new Color(0.40f, 0.80f, 0.40f), baseWeight = 25f, weightPerLuck = -0.10f },
        new() { id = "Rare",      displayName = "Rara",     color = new Color(0.30f, 0.55f, 0.95f), baseWeight = 10f, weightPerLuck =  0.15f },
        new() { id = "Epic",      displayName = "Épica",    color = new Color(0.75f, 0.35f, 0.95f), baseWeight =  4f, weightPerLuck =  0.15f },
        new() { id = "Legendary", displayName = "Lendária", color = new Color(0.99f, 0.75f, 0.20f), baseWeight =  1f, weightPerLuck =  0.10f },
    };

        public int Count => rarities.Count;

        public RarityDto GetRaw(int index)
        {
            if (rarities == null || rarities.Count == 0) return null;
            return rarities[Mathf.Clamp(index, 0, rarities.Count - 1)];
        }
    }
}

