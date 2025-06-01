using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public PlayerWeapon playerWeapon;
    public CameraBehaviour cam;

    public enum WeaponType
    {
        basic,
        shotgun,
        rico
    }

    BoxCollider2D bc;

    public WeaponType weaponType;

    [Header("Stats")]
    public WeaponStats weaponStats;
    public bool isPrimary;

    [Header("Bullet Info")]
    public Vector2 velocity;
    public float currentDamage;
    public float currentRange;

    [Header("Pierce")]
    public float currentPierceCooldown;

    [Header("Ray check")]
    public bool isColliding;
    public bool isOffScreen;
    public bool isActive;
    public LayerMask groundMask;
    public LayerMask enemyMask;

    [Header("Rico")]
    public int maxBounceIncrement;


    private void Awake()
    {
        bc = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraBehaviour>();
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        transform.position += (Vector3)velocity * Time.fixedDeltaTime; // Update projectile position
        currentRange -= velocity.magnitude * Time.fixedDeltaTime;

        if (currentPierceCooldown > 0)
        {
            currentPierceCooldown -= Time.fixedDeltaTime;
            if (currentPierceCooldown <= 0)
            {
                bc.enabled = true;
            }
        }

        if (isColliding)
        {
            if (!weaponStats.isPiercing)
            {
                DeActivate();
                return;
            }
            else
            {
                bc.enabled = false;
                currentPierceCooldown = weaponStats.pierceCooldown;
            }
        }
        if (isOffScreen)
        {
            isOffScreen = false;
            DeActivate();
            return;
        }

        isColliding = Physics2D.OverlapBox(transform.position, bc.size, transform.eulerAngles.z, groundMask | enemyMask);

        switch (weaponType)
        {
            case WeaponType.rico:
                RicoBehaviour();
                break;
        }

        if (currentRange < 0) DeActivate();
    }

    private void OnBecameInvisible()
    {
        if (weaponType == WeaponType.rico && currentRange > 0) return;
        if (isActive) isOffScreen = true;
    }

    void DeActivate()
    {
        isActive = false;

        isColliding = false;
        velocity = Vector2.zero;

        if (isPrimary) playerWeapon?.ReturnBulletToPrimaryPool(this);
        else playerWeapon?.ReturnBulletSecondaryToPool(this);

        gameObject.SetActive(false);
    }

    public void SetVelocity(Vector2 direction, int offsetIndex)
    {
        isActive = true;

        velocity = direction.normalized * weaponStats.projectileSpeed; // Set projectile speed
        velocity = Quaternion.Euler(0, 0, weaponStats.angleVarianceList[offsetIndex % weaponStats.angleVarianceList.Length]) * velocity; // Modify projectile direction based on variance index

        float angle = Mathf.Round(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        UpdateSpriteAngle();

        transform.position += Quaternion.Euler(0, 0, angle) * weaponStats.offsetList[offsetIndex];

        currentDamage = weaponStats.damage;
        currentRange = weaponStats.range;

        maxBounceIncrement = 4;
    }

    void UpdateSpriteAngle()
    {
        Vector2 direction = velocity.normalized;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    void RicoBehaviour()
    {
        isColliding = Physics2D.OverlapCircle(transform.position, bc.size.x * 0.5f, enemyMask);

        RaycastHit2D horizontalRay = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(velocity.x), Mathf.Abs(velocity.x) * Time.fixedDeltaTime + bc.size.x * 0.51f, groundMask);
        RaycastHit2D verticalRay = Physics2D.Raycast(transform.position, Vector2.up * Mathf.Sign(velocity.y), Mathf.Abs(velocity.y) * Time.fixedDeltaTime + bc.size.x * 0.51f, groundMask);

        if (velocity.x != 0)
        {
            if (horizontalRay.collider != null)
            {
                StartCoroutine(RicoBounce(0, (horizontalRay.distance - 0.1f) * Mathf.Sign(velocity.x)));
            }
        }

        if (velocity.y != 0)
        {
            if (verticalRay.collider != null)
            {
                StartCoroutine(RicoBounce(1, (verticalRay.distance - 0.1f) * Mathf.Sign(velocity.y)));
            }
        }
    }

    IEnumerator RicoBounce(int direction, float distanceFromRebound)
    {
        if (direction == 0)
        {
            if (maxBounceIncrement > 0)
            {
                velocity.x *= -1.25f;
                velocity.y *= 1.25f;
                currentDamage += 1f;
                currentRange += 2f;
                maxBounceIncrement--;
            }
            else
            {
                velocity.x *= -1;
                velocity.y *= 1;
            }
        }
        else
        {
            if (maxBounceIncrement > 0)
            {
                velocity.x *= 1.25f;
                velocity.y *= -1.25f;
                currentDamage += 1f;
                currentRange += 2f;
                maxBounceIncrement--;
            }
            else
            {
                velocity.x *= 1;
                velocity.y *= -1;
            }
        }
        UpdateSpriteAngle();

        yield return new WaitForFixedUpdate();

        if (direction == 0) transform.position += Vector3.right * distanceFromRebound; // Horizontal rebound offset fix
        else  transform.position += Vector3.up * distanceFromRebound; // Vertical rebound offset fix
    }
}
