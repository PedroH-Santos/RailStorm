using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    [DefaultExecutionOrder(-10)]
    public class PlayerStatsAggregator : MonoBehaviour
    {
        [Header("Vida")]
        [SerializeField] int _hp = 100;
        [SerializeField] int _maxHp = 100;

        public int HP
        {
            get => _hp;
            set => _hp = Mathf.Clamp(value, 0, _maxHp);
        }

        public int MaxHP
        {
            get => _maxHp;
            set => _maxHp = Mathf.Max(1, value);
        }

        [Header("Movimento")]
        [SerializeField] float _moveSpeed = 6f;

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0f, value);
        }

        [Header("Economia")]
        [SerializeField] int _coins = 50;

        public int Coins
        {
            get => _coins;
            set => _coins = Mathf.Max(0, value);
        }

        [Header("Sorte")]
        [Range(0f, 100f)]
        [SerializeField] float _luckPercent = 0f;

        public float LuckPercent
        {
            get => _luckPercent;
            set => _luckPercent = Mathf.Clamp(value, 0f, 100f);
        }

        public enum StatKey
        {
            HP,
            MaxHP,
            MoveSpeed,
            LuckPercent,
            Coins,
        }

        public class StatDescriptor
        {
            public StatKey Key;
            public string Label;
            public string Group;
            public Func<string> GetValue;
        }

        public IReadOnlyList<StatDescriptor> AllStats => _stats;
        readonly List<StatDescriptor> _stats = new();

        void Awake() => RegisterDisplayStats();

        void RegisterDisplayStats()
        {
            Add(StatKey.HP, "Vida", "LifeContainer", () => $"{HP} / {MaxHP}");
            Add(StatKey.MoveSpeed, "Velocidade", "PlayerContainer", () => $"{MoveSpeed:F1}");
            Add(StatKey.LuckPercent, "Sorte", "PlayerContainer", () => $"{LuckPercent:F0}%");
            Add(StatKey.Coins, "Moedas", "PlayerContainer", () => $"{Coins}");
        }

        void Add(StatKey key, string label, string group, Func<string> getValue)
            => _stats.Add(new StatDescriptor { Key = key, Label = label, Group = group, GetValue = getValue });
    }
}