using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var script = animator.GetComponent<EnemyAnimationController>();
        script.SetAttack(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var animatorscript = animator.GetComponent<EnemyAnimationController>();
        var enemyScript = animator.GetComponent<Enemy>();
        animatorscript.SetAttack(false);
        enemyScript.ThrowProjectile();

    }
}
