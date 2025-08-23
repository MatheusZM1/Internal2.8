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
        bubble,
        spike,
        homer,
        mini,
        sharpEX,
        ricoEX,
        sweepEX,
        bubbleEX,
        spikeEX,
        homerEX,
        miniEX
    }

    BoxCollider2D bc;

    public WeaponType weaponType;

    [Header("Stats")]
    public WeaponStats weaponStats;
    public bool isPrimary;
    public bool isEX;

    [Header("Bullet Info")]
    public Vector2 velocity;
    public float currentDamage;
    public float damageMultiplier;
    public float currentRange;
    public float currentDuration;

    [Header("Pierce")]
    public float currentPierceCooldown;
    public float freezeMovementDuration;

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
    public LayerMask torchMask;
    public Sprite bubbleSprite;
    public Sprite hotBubbleSprite;
    float extraInitialVelocity;

    [Header("Spike")]
    public float spikeGravity;

    [Header("Homer")]
    public float maxRotationSpeed;
    EnemyHP targetEnemy;

    private void Awake()
    {
        bc = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        Actions.resetProjectiles += DeActivate;
    }

    private void OnDisable()
    {
        Actions.resetProjectiles -= DeActivate;
    }

    private void Update()
    {
        switch (weaponType)
        {
            case WeaponType.rico:
                RicoBehaviour();
                break;

            case WeaponType.spike:
            case WeaponType.spikeEX:
                SpikeBehaviour();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        if (freezeMovementDuration <= 0)
        {
            transform.position += (Vector3)velocity * Time.fixedDeltaTime; // Update projectile position
            currentRange -= velocity.magnitude * Time.fixedDeltaTime;
        }
        else
        {
            freezeMovementDuration -= Time.fixedDeltaTime;
        }

        if (currentDuration > 0)
        {
            currentDuration -= Time.deltaTime;
            if (currentDuration <= 0) // Disable projectile after it's life time ends
            {
                DeActivate();
                return;
            }
        }

        if (currentPierceCooldown > 0)
        {
            currentPierceCooldown -= Time.fixedDeltaTime;
            if (currentPierceCooldown <= 0.001f) // Disable projectile hitbox during pierce cooldown
            {
                currentPierceCooldown = 0f;
                bc.enabled = true;
            }
        }

        if (isColliding)
        {
            if (!weaponStats.isPiercing) // Disable projectile on collision unless it can pierce
            {
                DeActivate();
                return;
            }
        }
        if (isOffScreen) // Disable off screen projectiles (not spikes)
        {
            isOffScreen = false;
            if (weaponType != WeaponType.spike && weaponType != WeaponType.bubbleEX && weaponType != WeaponType.spikeEX) DeActivate();
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

            case WeaponType.spike:
                SpikeBehaviourFixed();
                break;

            case WeaponType.homer:
                HomerBehaviourFixed();
                break;

            case WeaponType.ricoEX:
                RicoBehaviourFixed();
                break;

            case WeaponType.bubbleEX:
                BubbleEXBehaviourFixed();
                break;

            case WeaponType.spikeEX:
                SpikeBehaviourEXFixed();
                break;

            case WeaponType.homerEX:
                HomerBehaviourFixed();
                break;
        }

        if (currentRange <= 0f) DeActivate();
    }

    public void OffscreenFunc()
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
            switch (weaponType)
            {
                case WeaponType.sharpEX:
                    freezeMovementDuration = weaponStats.pierceCooldown - Time.fixedDeltaTime;
                    currentDamage -= 4f / 3f;
                    if (currentDamage < 0.1f) DeActivate();
                    break;
            }
        }
    }

    void DeActivate()
    {
        isActive = false;

        isColliding = false;
        velocity = Vector2.zero;

        if (!isEX)
        {
            if (isPrimary) playerWeapon?.ReturnBulletToPrimaryPool(this);
            else playerWeapon?.ReturnBulletSecondaryToPool(this);
        }
        else
        {
            if (isPrimary) playerWeapon?.ReturnEXBulletToPrimaryPool(this);
            else playerWeapon?.ReturnEXBulletSecondaryToPool(this);
        }

        if (weaponType == WeaponType.bubble) transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = bubbleSprite; // Reset hot bubbles

        gameObject.SetActive(false);
    }

    public void SpawnSetVelocity(Vector2 direction, int offsetIndex, bool flipY = false)
    {
        isActive = true;

        float randomSpeedVariance = Random.Range(weaponStats.speedVarianceMin, weaponStats.speedVarianceMax);
        velocity = direction.normalized * (weaponStats.projectileSpeed + randomSpeedVariance); // Set projectile speed
        velocity = Quaternion.Euler(0, 0, weaponStats.angleVarianceList[offsetIndex % weaponStats.angleVarianceList.Length]) * velocity; // Modify projectile direction based on variance index

        float angle = Mathf.Round(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        if (flipY && !weaponStats.doNotFlipSprite) transform.localScale = new Vector3(1, -1, 1);
        UpdateSpriteAngle();

        transform.position += Quaternion.Euler(0, 0, angle) * (weaponStats.offsetList[offsetIndex] + Vector2.up * Random.Range(weaponStats.yVarianceMin, weaponStats.yVarianceMax));
        transform.localScale = Vector3.one * Random.Range(weaponStats.scaleMin, weaponStats.scaleMax);

        bc.enabled = true;
        currentPierceCooldown = 0f;

        currentDamage = weaponStats.damage;
        currentRange = weaponStats.range + Random.Range(weaponStats.rangeVarianceMin, weaponStats.rangeVarianceMax);
        currentDuration = weaponStats.projectileDuration;

        if (weaponStats.doNotRotateSprite) spriteTransform.eulerAngles = Vector3.zero;

        maxBounceIncrement = 4;

        extraInitialVelocity = randomSpeedVariance;

        if (weaponType == WeaponType.homer || weaponType == WeaponType.homerEX)
        {
            var nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null) targetEnemy = nearestEnemy.GetComponent<EnemyHP>();
            else targetEnemy = null;
        }
    }

    GameObject FindNearestEnemy()
    {
        // Locate all enemy objects
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var obj in objs)
        {
            if (obj.GetComponent<EnemyHP>().health <= 0) continue;

            float dist = Vector2.Distance(pos, obj.transform.position);
            if (dist < minDist && dist < 17f) // Determine closest enemy within limit
            {
                minDist = dist;
                nearest = obj;
            }
        }

        return nearest;
    }

    void UpdateSpriteAngle()
    {
        if (weaponStats.doNotFlipSprite) return;
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
        RaycastHit2D verticalRay = Physics2D.Raycast(transform.position, Vector2.up * Mathf.Sign(velocity.y), Mathf.Abs(velocity.y) * Time.fixedDeltaTime + bc.size.y * 0.51f, groundMask);

        if (velocity.x != 0) // Horiozntal bounce
        {
            if (horizontalRay.collider != null)
            {
                StartCoroutine(RicoBounce(0, (horizontalRay.distance - bc.size.x * 0.5f) * Mathf.Sign(velocity.x)));
            }
        }

        if (velocity.y != 0) // Vertical bounce
        {
            if (verticalRay.collider != null)
            {
                StartCoroutine(RicoBounce(1, (verticalRay.distance - bc.size.y * 0.5f) * Mathf.Sign(velocity.y)));
            }
        }

        if (velocity.x != 0 && velocity.y != 0 && horizontalRay.collider != null && verticalRay.collider != null) // Diagonal bounce
        {
            float diagonalSize = Mathf.Sqrt(bc.size.x * bc.size.x + bc.size.y * bc.size.y);
            RaycastHit2D diagonalRay = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(velocity.x) + Vector2.up * Mathf.Sign(velocity.y), Mathf.Abs(velocity.magnitude) * Time.fixedDeltaTime + diagonalSize * 0.51f, groundMask);

            if (diagonalRay.collider != null)
            {
                StartCoroutine(RicoBounce(2, diagonalRay.distance - diagonalSize * 0.5f));
            }
        }
    }

    void BubbleBehaviourFixed()
    {
        velocity = velocity.normalized * (currentRange + 3f + extraInitialVelocity);
        if (currentRange <= 0.05) currentRange = -1f;

        bool isTorched = Physics2D.OverlapBox(transform.position, bc.size, transform.eulerAngles.z, torchMask);

        if (isTorched && currentDamage < 2)
        {
            currentDamage = 2;
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hotBubbleSprite;
        }
    }

    void BubbleEXBehaviourFixed()
    {
        RaycastHit2D verticalRay = Physics2D.Raycast(transform.position, -Vector2.up, Mathf.Abs(velocity.y) * Time.fixedDeltaTime + bc.size.y * 0.51f, groundMask);
        if (verticalRay.collider != null)
        {
            velocity.y = 0;
            transform.position = new Vector2(transform.position.x, Mathf.Round((verticalRay.point.y + bc.size.y * 0.5f) * 32f) / 32f);
        }
        else
        {
            velocity.y -= spikeGravity * Time.fixedDeltaTime;
        }
    }

    void SpikeBehaviour()
    {
        spriteTransform.eulerAngles += Vector3.forward * 400f * Time.deltaTime;
    }

    void SpikeBehaviourFixed()
    {
        velocity.y -= spikeGravity * Time.fixedDeltaTime;
    }

    void SpikeBehaviourEXFixed()
    {
        isColliding = false;
        velocity.y -= spikeGravity * Time.fixedDeltaTime;

        RaycastHit2D horizontalRay = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(velocity.x), Mathf.Abs(velocity.x) * Time.fixedDeltaTime + bc.size.x * 0.51f, groundMask);
        RaycastHit2D verticalRay = Physics2D.Raycast(transform.position, -Vector2.up, Mathf.Abs(velocity.y) * Time.fixedDeltaTime + bc.size.y * 0.51f, groundMask);

        if (horizontalRay.collider != null)
        {
            velocity.x *= -1;
        }
        if (verticalRay.collider != null)
        {
            velocity.y = Mathf.Max(7, -velocity.y * 0.75f);
        }
    }

    void HomerBehaviourFixed()
    {
        if (targetEnemy != null)
        {
            if (targetEnemy.health <= 0)
            {
                targetEnemy = null;
                return;
            }
            Vector2 dir = (targetEnemy.transform.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float currentAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxRotationSpeed * Time.deltaTime);
            velocity = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)) * velocity.magnitude;
        }

        Vector2 direction = velocity.normalized;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        velocity *= 1.008f;
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
            }
        }
        else if (direction == 1)
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
                velocity.y *= -1;
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
                velocity.x *= -1;
                velocity.y *= -1;
            }
        }
        UpdateSpriteAngle();

        yield return new WaitForFixedUpdate();

        if (direction == 0) transform.position += Vector3.right * distanceFromRebound; // Horizontal rebound offset fix
        else if(direction == 1)  transform.position += Vector3.up * distanceFromRebound; // Vertical rebound offset fix
        else
        {
            transform.position += new Vector3(1 * Mathf.Sign(velocity.x), 1 * -Mathf.Sign(velocity.y)) * distanceFromRebound;
        }
    }
}
