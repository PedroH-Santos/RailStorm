using DG.Tweening;
using UnityEngine;

public static class UITweenExtensions
{
    public static T AsUI<T>(this T tween, GameObject owner) where T : Tween
        => tween.SetUpdate(true).SetLink(owner, LinkBehaviour.KillOnDisable);
}
