using UnityEngine;


[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;

    [Header("Animator Parameters")]
    private string speedParameter = "speed";
    private string isWalkingParameter = "isWalking";
    private string isAttackingParameter = "isAttacking";

    [Header("Settings")]
    [Tooltip("Minimum horizontal speed to consider the enemy as walking")]
    [SerializeField] private float walkingThreshold = 0.1f;

    private int hashSpeed;
    private int hashIsWalking;
    private int hashIsAttacking;

    private bool isAttacking; 

    private float currentSpeed;
    private Vector3 lastPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        hashSpeed = Animator.StringToHash(speedParameter);
        hashIsWalking = Animator.StringToHash(isWalkingParameter);
        hashIsAttacking = Animator.StringToHash(isAttackingParameter);
        lastPosition = transform.position;
        isAttacking = false;

    }

    // Use LateUpdate so we sample the final position after movement logic in Update
    private void LateUpdate()
    {
        currentSpeed = GetCurrentSpeed();

        animator.SetFloat(hashSpeed, currentSpeed);
        animator.SetBool(hashIsWalking, currentSpeed > walkingThreshold);

        lastPosition = transform.position;

    }

    public void OnAttack()
    {
        animator.SetTrigger(hashIsAttacking);
    }

    public bool GetAttack()
    {
        return isAttacking;
    }

    public void SetAttack(bool attack)
    {
        isAttacking = attack;
    }


    public float GetCurrentSpeed()
    {
        float dt = Time.deltaTime;
        if (dt <= 0) return 0f; 
        Vector3 delta = transform.position - lastPosition;

        float speed = delta.magnitude / dt;

        return speed;
    }
}
