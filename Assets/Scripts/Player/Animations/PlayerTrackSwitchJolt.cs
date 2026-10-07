using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerTrackSwitchJolt : MonoBehaviour
{
    PlayerAnimationController _animation;
    PlayerController _cart;

    void Awake()
    {
        _animation = GetComponent<PlayerAnimationController>();
        _cart = GetComponentInParent<PlayerController>();
    }

    void OnEnable()
    {
        if (_cart != null) _cart.OnSplineSwitched += _animation.PlayJolt;
    }

    void OnDisable()
    {
        if (_cart != null) _cart.OnSplineSwitched -= _animation.PlayJolt;
    }
}
