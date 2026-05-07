using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{

    private Animator animator;

    private readonly int hashOnAttack = Animator.StringToHash("onAttack");
    private readonly int hashAttackIndex = Animator.StringToHash("attackIndex");

    private int currentAttack;

    private void Awake()
    {
        currentAttack = 0;
        animator = GetComponent<Animator>();
    }

    public void PlayAttackAnimation()
    {
        animator.SetInteger(hashAttackIndex, currentAttack);
        animator.SetTrigger(hashOnAttack);
        currentAttack = (currentAttack + 1) % 2;


    }

}
