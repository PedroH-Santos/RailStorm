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

        [Header("Movimento")]
        [SerializeField] float _moveSpeed = 6f;
        [SerializeField] float _idleSpeed = 0.5f;
        [SerializeField] float _acceleration = 6f;
        [SerializeField] float _deceleration = 4f;
        [SerializeField] float _rotationSmoothTime = 0.12f;

        [Header("Economia")]
        [SerializeField] int _coins = 50;

        [Header("Sorte")]
        [Range(0f, 100f)]
        [SerializeField] float _luckPercent = 0f;

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

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0f, value);
        }

        public float IdleSpeed
        {
            get => _idleSpeed;
            set => _idleSpeed = Mathf.Max(0f, value);
        }

        public float Acceleration
        {
            get => _acceleration;
            set => _acceleration = Mathf.Max(0f, value);
        }

        public float Deceleration
        {
            get => _deceleration;
            set => _deceleration = Mathf.Max(0f, value);
        }

        public float RotationSmoothTime
        {
            get => _rotationSmoothTime;
            set => _rotationSmoothTime = Mathf.Max(0f, value);
        }

        public int Coins
        {
            get => _coins;
            set => _coins = Mathf.Max(0, value);
        }

        public float LuckPercent
        {
            get => _luckPercent;
            set => _luckPercent = Mathf.Clamp(value, 0f, 100f);
        }


        readonly List<StatDescriptor> _stats = new();
        public IReadOnlyList<StatDescriptor> AllStats => _stats;

        void Awake() => RegisterDisplayStats();

        void RegisterDisplayStats()
        {
            Add(EStatKey.HP, "Vida", "LifeContainer", () => $"{HP} / {MaxHP}");
            Add(EStatKey.MoveSpeed, "Velocidade", "PlayerContainer", () => $"{MoveSpeed:F1}");
            Add(EStatKey.LuckPercent, "Sorte", "PlayerContainer", () => $"{LuckPercent:F0}%");
            Add(EStatKey.Coins, "Moedas", "PlayerContainer", () => $"{Coins}");
        }

        void Add(EStatKey key, string label, string group, Func<string> getValue) =>
            _stats.Add(new StatDescriptor { Key = key, Label = label, Group = group, GetValue = getValue });

        public void SpendCoins(int amount) => Coins -= amount;
    }
}