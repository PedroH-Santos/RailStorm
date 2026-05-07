using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{

    [Header("Movement")]
    private Transform player;
    public float rotationSpeed = 10f;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    public float attackRange = 5f;
    public float attackCooldown = 1f;
    public int attackDamage = 15;
    private float attackTimer = 0f;

    
    private NavMeshAgent navAgent;
    private EnemyAnimationController animationController;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animationController = GetComponent<EnemyAnimationController>();

        navAgent.stoppingDistance = attackRange;
        navAgent.updateRotation = false;

    }

    void Update()
    {
        FollowPlayer();
        OnAttack();
        ApplySmoothRotation();
      
    }



    void ApplySmoothRotation()
    {
        Vector3 direction;

        if(player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);


        if (distance <= attackRange)
        {
            direction = (player.position - transform.position).normalized;
        }
        else
        {
            direction = navAgent.desiredVelocity.normalized;
        }

        if (direction != Vector3.zero)
        {
            direction.y = 0; 
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;
        if (animationController == null) return;

        if (animationController.GetAttack())
        {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
            return;
        }

        navAgent.isStopped = false;


        if (navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(player.position);
        }





    }
    void OnAttack()
    {
        if (player == null) return;
        if(animationController == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown && distance <= attackRange)
        {
            animationController.OnAttack();
            attackTimer = 0f;
        }
    }

    public void ThrowProjectile()
    {

        GameObject obj = Instantiate(projectilePrefab, firePoint.position, projectilePrefab.transform.rotation);
        
        AxeProjectile proj = obj.GetComponent<AxeProjectile>();
       
        if (proj != null && player != null)
        {
            Vector3 dir = (player.position - firePoint.position).normalized;

            proj.Init(dir, attackDamage);
        }
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }
}
