using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public PlayerWeapon playerWeapon;

    public enum WeaponType
    {
        sharp,
        rico,
        sweep,
        bubble
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

    [Header("Visuals")]
    public Transform spriteTransform;

    [Header("Rico")]
    public float spriteRotationDelay;
    float currentSpriteRotationDelay;
    int maxBounceIncrement;

    [Header("Bubble")]
    float extraInitialVelocity;


    private void Awake()
    {
        bc = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        Actions.levelReset += DeActivate;
    }

    private void OnDisable()
    {
        Actions.levelReset -= DeActivate;
    }

    private void Update()
    {
        switch (weaponType)
        {
            case WeaponType.rico:
                RicoBehaviour();
                break;
        }
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
        }
        if (isOffScreen)
        {
            isOffScreen = false;
            DeActivate();
            return;
        }

        isColliding = Physics2D.OverlapBox(transform.position, bc.size, transform.eulerAngles.z, groundMask);

        switch (weaponType)
        {
            case WeaponType.rico:
                RicoBehaviourFixed();
                break;

            case WeaponType.bubble:
                BubbleBehaviourFixed();
                break;
        }

        if (currentRange <= 0f) DeActivate();
    }

    private void OnBecameInvisible()
    {
        if (weaponType == WeaponType.rico && currentRange > 0) return;
        if (isActive) isOffScreen = true;
    }

    public void CollideWithEnemy()
    {
        if (!weaponStats.isPiercing)
        {
            DeActivate();
            return;
        }
        else if (bc.enabled)
        {
            bc.enabled = false;
            currentPierceCooldown = weaponStats.pierceCooldown;
        }
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

    public void SpawnSetVelocity(Vector2 direction, int offsetIndex, bool flipY = false)
    {
        isActive = true;

        float randomVariance = Random.Range(weaponStats.speedVarianceMin, weaponStats.speedVarianceMax);
        velocity = direction.normalized * (weaponStats.projectileSpeed + randomVariance); // Set projectile speed
        velocity = Quaternion.Euler(0, 0, weaponStats.angleVarianceList[offsetIndex % weaponStats.angleVarianceList.Length]) * velocity; // Modify projectile direction based on variance index

        float angle = Mathf.Round(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        if (flipY) transform.localScale = new Vector3(1, -1, 1);
        UpdateSpriteAngle();

        transform.position += Quaternion.Euler(0, 0, angle) * (weaponStats.offsetList[offsetIndex] + Vector2.up * Random.Range(weaponStats.yVarianceMin, weaponStats.yVarianceMax));
        transform.localScale = Vector2.one * Random.Range(weaponStats.scaleMin, weaponStats.scaleMax);

        bc.enabled = true;
        currentPierceCooldown = 0f;

        currentDamage = weaponStats.damage;
        currentRange = weaponStats.range;

        if (weaponStats.doNotRotateSprite) spriteTransform.eulerAngles = Vector3.zero;

        maxBounceIncrement = 4;

        extraInitialVelocity = randomVariance;
    }

    void UpdateSpriteAngle()
    {
        Vector2 direction = velocity.normalized;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        if (!Mathf.Approximately(direction.x, 0))
        {
            transform.localScale = new Vector3(1 * Mathf.Sign(direction.x), 1, 1);
            if (direction.x < 0)
            {
                transform.eulerAngles += new Vector3(0, 0, 180);
            }
        }
    }

    void RicoBehaviour()
    {
        currentSpriteRotationDelay -= Time.deltaTime;
        if (currentSpriteRotationDelay < 0)
        {
            currentSpriteRotationDelay = spriteRotationDelay;
            spriteTransform.eulerAngles -= new Vector3(0, 0, 45 * Mathf.Sign(transform.localScale.x));
        }
    }

    void RicoBehaviourFixed()
    {
        isColliding = false;

        RaycastHit2D horizontalRay = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(velocity.x), Mathf.Abs(velocity.x) * Time.fixedDeltaTime + bc.size.x * 0.51f, groundMask);
        RaycastHit2D verticalRay = Physics2D.Raycast(transform.position, Vector2.up * Mathf.Sign(velocity.y), Mathf.Abs(velocity.y) * Time.fixedDeltaTime + bc.size.x * 0.51f, groundMask);

        if (velocity.x != 0)
        {
            if (horizontalRay.collider != null)
            {
                StartCoroutine(RicoBounce(0, (horizontalRay.distance - bc.size.x * 0.5f) * Mathf.Sign(velocity.x)));
            }
        }

        if (velocity.y != 0)
        {
            if (verticalRay.collider != null)
            {
                StartCoroutine(RicoBounce(1, (verticalRay.distance - bc.size.y * 0.5f) * Mathf.Sign(velocity.y)));
            }
        }
    }

    void BubbleBehaviourFixed()
    {
        velocity = velocity.normalized * (currentRange + 3f + extraInitialVelocity);
        if (currentRange <= 0.05) currentRange = -1f;
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
