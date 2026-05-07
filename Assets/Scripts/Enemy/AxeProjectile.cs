using UnityEngine;

public class AxeProjectile : MonoBehaviour
{

    [SerializeField] private float speed = 25f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody rb;
    private float lifeTimeTimer = 0f;
    private bool initialized = false;
    private int damage = 20;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (trailParticles != null) trailParticles.Play();
    }

    public void Init(Vector3 direction, int damage)
    {
        this.damage = damage;

         SetDestination(direction);

    }

    void SetDestination(Vector3 direction)
    {

        if (rb == null) rb = GetComponent<Rigidbody>();

        Vector3 moveDir = direction;

        moveDir.y = 0f;

        if (moveDir.sqrMagnitude < 0.0001f) moveDir = transform.right;

        moveDir = moveDir.normalized;

        rb.linearVelocity = moveDir * speed;

        initialized = true;
    }


    void Update()
    {
        if (!initialized) return;
        
        lifeTimeTimer += Time.deltaTime;

        if (lifeTimeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = other.gameObject;

        if (target.CompareTag("Player"))
        {

            if (!target.TryGetComponent<LifeSystem>(out var lifeSystem))
            {
                return;
            }

            lifeSystem.Damage(damage);

        }



    }

}
