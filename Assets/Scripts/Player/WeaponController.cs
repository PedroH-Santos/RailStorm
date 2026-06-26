using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{

    [Header("Aim")]
    public Transform player; 
    public LayerMask groundMask;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int bulletDamage = 15;
    public float bulletSpeed = 25f;
    public float bulletRange = 15f;
    public float bulletfireRate = 0.3f;
    
    private float fireTimer;
    private float _timer;
    private Vector3 lastAimDirection;

    [Header("Animation")]
    private PlayerAnimationController playerAnimation;
    [SerializeField] private float animationInterval = 0.5f;

    void Start()
    {
        playerAnimation = player.gameObject.GetComponent<PlayerAnimationController>();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        lastAimDirection = Vector3.zero;
        AimAtMouse();
        HandleAutoFire();
    }

    void AimAtMouse()
    {
        Vector3 aimDirection = Vector3.zero;

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            if (stick.sqrMagnitude > 0.1f) 
            {
                aimDirection = new Vector3(stick.x, 0f, stick.y);
            }
        }

        if (aimDirection == Vector3.zero && Mouse.current != null)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mouseScreenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                Vector3 lookPoint = hit.point;
                lookPoint.y = player.position.y;
                aimDirection = lookPoint - player.position;
            }
        }

        if (aimDirection.sqrMagnitude < 0.001f) return;
        
        lastAimDirection = aimDirection;
        
        Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
        
        player.rotation = targetRotation;
    }

    void HandleAutoFire()
    {
        fireTimer += Time.deltaTime;

        if (Keyboard.current.jKey.isPressed && fireTimer >= bulletfireRate)
        {
            ExecuteAnimation();
            Fire();
            fireTimer = 0f;
        }
    }

    void Fire()
    {
        GameObject bulletObj = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        var direction = lastAimDirection != Vector3.zero ? lastAimDirection.normalized : player.forward;

        PlayerFireballController bullet = bulletObj.GetComponent<PlayerFireballController>();
        bullet.Init(direction, bulletSpeed, bulletRange, bulletDamage);
    }

    void ExecuteAnimation()
    {
        if (_timer >= animationInterval)
        {
            if (playerAnimation != null)
            {
                playerAnimation.PlayAttackAnimation();
            }
            _timer = 0f;
        }

    }

}
