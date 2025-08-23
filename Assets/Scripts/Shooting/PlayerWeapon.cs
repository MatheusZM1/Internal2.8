using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    PlayerMovement playerScript;
    InputScript inputScript;

    [Header("Player Weapon")]
    public ProjectileBehaviour primaryBulletPrefab;
    public ProjectileBehaviour secondaryBulletPrefab;
    public ProjectileBehaviour EXPrimaryBulletPrefab;
    public ProjectileBehaviour EXSecondaryBulletPrefab;

    [Header("Shooting")]
    public bool primaryWeaponSelected;
    public float fireCooldown;
    public float playerSpeedModifier;
    public int primaryOffsetIndex;
    public int secondaryOffsetIndex;

    [Header("Alt Shooting")]
    public float primaryCooldown;
    public float secondaryCooldown;

    private Queue<ProjectileBehaviour> primaryBulletPool = new Queue<ProjectileBehaviour>();
    private Queue<ProjectileBehaviour> secondaryBulletPool = new Queue<ProjectileBehaviour>();
    private Queue<ProjectileBehaviour> EXPrimaryBulletPool = new Queue<ProjectileBehaviour>();
    private Queue<ProjectileBehaviour> EXSecondaryBulletPool = new Queue<ProjectileBehaviour>();

    private void Awake()
    {
        playerScript = GetComponent<PlayerMovement>();
        inputScript = GetComponent<InputScript>();
        primaryWeaponSelected = true;
    }

    private void Start()
    {
        LoadWeapons();
    }

   public void LoadWeapons()
    {
        primaryWeaponSelected = true;
        primaryOffsetIndex = 0;
        secondaryOffsetIndex = 0;
        primaryCooldown = 0;
        secondaryCooldown = 0;

        while (primaryBulletPool.Count > 0)
        {
            var bullet = primaryBulletPool.Dequeue();
            if (bullet != null) Destroy(bullet.gameObject);
        }

        while (secondaryBulletPool.Count > 0)
        {
            var bullet = secondaryBulletPool.Dequeue();
            if (bullet != null) Destroy(bullet.gameObject);
        }

        while (EXPrimaryBulletPool.Count > 0)
        {
            var bullet = EXPrimaryBulletPool.Dequeue();
            if (bullet != null) Destroy(bullet.gameObject);
        }

        while (EXSecondaryBulletPool.Count > 0)
        {
            var bullet = EXSecondaryBulletPool.Dequeue();
            if (bullet != null) Destroy(bullet.gameObject);
        }

        primaryBulletPool.Clear();
        secondaryBulletPool.Clear();
        EXPrimaryBulletPool.Clear();
        EXSecondaryBulletPool.Clear();

        primaryBulletPrefab = LoadoutManager.instance.GetPrimaryNormal(playerScript.isPlayerTwo);
        secondaryBulletPrefab = LoadoutManager.instance.GetSecondaryNormal(playerScript.isPlayerTwo);
        EXPrimaryBulletPrefab = LoadoutManager.instance.GetPrimaryEX(playerScript.isPlayerTwo);
        EXSecondaryBulletPrefab = LoadoutManager.instance.GetSecondaryEX(playerScript.isPlayerTwo);

        // Initialize the bullet pool
        for (int i = 0; i < primaryBulletPrefab.weaponStats.instanceCount; i++)
        {
            ProjectileBehaviour bullet = Instantiate(primaryBulletPrefab);
            bullet.isPrimary = true;
            bullet.isEX = false;
            bullet.playerWeapon = this;
            if (!playerScript.isPlayerTwo) bullet.damageMultiplier = LoadoutManager.instance.buffP1 == 0 ? 1.1f : 1;
            else bullet.damageMultiplier = LoadoutManager.instance.buffP2 == 0 ? 1.1f : 1;

            bullet.gameObject.SetActive(false); // Deactivate initially
            primaryBulletPool.Enqueue(bullet); // Add to pool
        }

        for (int i = 0; i < secondaryBulletPrefab.weaponStats.instanceCount; i++)
        {
            ProjectileBehaviour bullet = Instantiate(secondaryBulletPrefab);
            bullet.isPrimary = false;
            bullet.isEX = false;
            bullet.playerWeapon = this;
            if (!playerScript.isPlayerTwo) bullet.damageMultiplier = LoadoutManager.instance.buffP1 == 0 ? 1.1f : 1;
            else bullet.damageMultiplier = LoadoutManager.instance.buffP2 == 0 ? 1.1f : 1;

            bullet.gameObject.SetActive(false); // Deactivate initially
            secondaryBulletPool.Enqueue(bullet); // Add to pool
        }

        for (int i = 0; i < EXPrimaryBulletPrefab.weaponStats.instanceCount; i++)
        {
            ProjectileBehaviour bullet = Instantiate(EXPrimaryBulletPrefab);
            bullet.isPrimary = true;
            bullet.isEX = true;
            bullet.playerWeapon = this;
            if (!playerScript.isPlayerTwo) bullet.damageMultiplier = LoadoutManager.instance.buffP1 == 0 ? 1.1f : 1;
            else bullet.damageMultiplier = LoadoutManager.instance.buffP2 == 0 ? 1.1f : 1;

            bullet.gameObject.SetActive(false); // Deactivate initially
            EXPrimaryBulletPool.Enqueue(bullet); // Add to pool
        }

        for (int i = 0; i < EXSecondaryBulletPrefab.weaponStats.instanceCount; i++)
        {
            ProjectileBehaviour bullet = Instantiate(EXSecondaryBulletPrefab);
            bullet.isPrimary = false;
            bullet.isEX = true;
            bullet.playerWeapon = this;
            if (!playerScript.isPlayerTwo) bullet.damageMultiplier = LoadoutManager.instance.buffP1 == 0 ? 1.1f : 1;
            else bullet.damageMultiplier = LoadoutManager.instance.buffP2 == 0 ? 1.1f : 1;

            bullet.gameObject.SetActive(false); // Deactivate initially
            EXSecondaryBulletPool.Enqueue(bullet); // Add to pool
        }
    }

    private void Update()
    {
        if (GameManagerScript.instance.gamePaused) return;

        if (!playerScript.isAlive || playerScript.inputLockedCooldown > 0) return;

        if ((Input.GetKey("a") && !playerScript.isPlayerTwo) || inputScript.GetActionHold("RTrigger"))
        {
            if (fireCooldown <= 0f) Shoot();
        }
        else
        {
            playerSpeedModifier = 1f;
        }
        if ((Input.GetKeyDown("s") && !playerScript.isPlayerTwo) || inputScript.GetActionDown("LBumper")) primaryWeaponSelected = !primaryWeaponSelected;

        if ((Input.GetKeyDown("d") && !playerScript.isPlayerTwo) || inputScript.GetActionDown("LTrigger"))
        {
            if (primaryWeaponSelected)
            {
                if (primaryCooldown <= 0f) EXShoot();
            }
            else
            {
                if (secondaryCooldown <= 0f) EXShoot();
            }
        }
    }

    private void FixedUpdate()
    {
        if (fireCooldown > 0) fireCooldown -= Time.fixedDeltaTime;
        if (primaryCooldown > 0) primaryCooldown -= Time.fixedDeltaTime;
        if (secondaryCooldown > 0) secondaryCooldown -= Time.fixedDeltaTime;
    }

    private void Shoot()
    {
        int bulletPoolCount = primaryWeaponSelected ? primaryBulletPool.Count : secondaryBulletPool.Count;

        int projectileCount = primaryWeaponSelected ? primaryBulletPrefab.weaponStats.projectileCount : secondaryBulletPrefab.weaponStats.projectileCount;

        for (int i = 0; i < projectileCount && bulletPoolCount > 0; i++)
        {
            // Get the bullet from the pool
            ProjectileBehaviour newBullet = primaryWeaponSelected ? primaryBulletPool.Dequeue() : secondaryBulletPool.Dequeue();

            int offsetIndex = primaryWeaponSelected ? primaryOffsetIndex : secondaryOffsetIndex;

            // Set bullet properties
            if (primaryWeaponSelected) // Increment primary weapon offset index
            {
                primaryOffsetIndex = (primaryOffsetIndex + 1) % newBullet.weaponStats.offsetList.Length;
            }
            else
            {
                secondaryOffsetIndex = (secondaryOffsetIndex + 1) % newBullet.weaponStats.offsetList.Length;
            }

            newBullet.transform.position = transform.position;

            // Set bullet velocity based on player's facing direction
            if (playerScript.directionFacing.y == 0)
            {
                newBullet.SpawnSetVelocity(playerScript.directionFacing, offsetIndex); // Shoot horizontally
            }
            else
            {
                if (playerScript.horizontal == 0)
                {
                    newBullet.SpawnSetVelocity(new Vector2(0, playerScript.directionFacing.y), offsetIndex, playerScript.directionFacing.x < 0 ? true : false); // Shoot vertically
                }
                else
                {
                    newBullet.SpawnSetVelocity(new Vector2(playerScript.horizontal, playerScript.directionFacing.y), offsetIndex); // Shoot diagonally
                }
            }

            // Activate the bullet
            newBullet.gameObject.SetActive(true);

            // Set the fire cooldown
            fireCooldown = newBullet.weaponStats.fireCooldown;
            playerSpeedModifier = newBullet.weaponStats.playerSpeedModifier;

            bulletPoolCount = primaryWeaponSelected ? primaryBulletPool.Count : secondaryBulletPool.Count;
        }
    }

    private void EXShoot()
    {
        int bulletPoolCount = primaryWeaponSelected ? EXPrimaryBulletPool.Count : EXSecondaryBulletPool.Count;

        int projectileCount = primaryWeaponSelected ? EXPrimaryBulletPrefab.weaponStats.projectileCount : EXSecondaryBulletPrefab.weaponStats.projectileCount;

        for (int i = 0; i < projectileCount && bulletPoolCount > 0; i++)
        {
            // Get the bullet from the pool
            ProjectileBehaviour newBullet = primaryWeaponSelected ? EXPrimaryBulletPool.Dequeue() : EXSecondaryBulletPool.Dequeue();

            newBullet.transform.position = transform.position;

            // Set bullet velocity based on player's facing direction
            if (playerScript.directionFacing.y == 0)
            {
                newBullet.SpawnSetVelocity(playerScript.directionFacing, i); // Shoot horizontally
            }
            else
            {
                if (playerScript.horizontal == 0)
                {
                    newBullet.SpawnSetVelocity(new Vector2(0, playerScript.directionFacing.y), i, playerScript.directionFacing.x < 0 ? true : false); // Shoot vertically
                }
                else
                {
                    newBullet.SpawnSetVelocity(new Vector2(playerScript.horizontal, playerScript.directionFacing.y), i); // Shoot diagonally
                }
            }

            // Activate the bullet
            newBullet.gameObject.SetActive(true);

            // Set the fire cooldown
            if (primaryWeaponSelected) primaryCooldown = newBullet.weaponStats.fireCooldown;
            else secondaryCooldown = newBullet.weaponStats.fireCooldown;

            bulletPoolCount = primaryWeaponSelected ? EXPrimaryBulletPool.Count : EXSecondaryBulletPool.Count;
        }

        ProjectileBehaviour bulletPrefab = primaryWeaponSelected ? EXPrimaryBulletPrefab : EXSecondaryBulletPrefab;

        if (bulletPrefab.weaponType == ProjectileBehaviour.WeaponType.sweepEX)
        {
            playerScript.isJumping = false;
            playerScript.velocity = Vector2.up * 20f;
        }
    }

    public void ReturnBulletToPrimaryPool(ProjectileBehaviour bullet)
    {
        bullet.gameObject.SetActive(false); // Deactivate the bullet
        primaryBulletPool.Enqueue(bullet); // Add the bullet back to the pool
    }

    public void ReturnBulletSecondaryToPool(ProjectileBehaviour bullet)
    {
        bullet.gameObject.SetActive(false); // Deactivate the bullet
        secondaryBulletPool.Enqueue(bullet); // Add the bullet back to the pool
    }

    public void ReturnEXBulletToPrimaryPool(ProjectileBehaviour bullet)
    {
        bullet.gameObject.SetActive(false); // Deactivate the bullet
        EXPrimaryBulletPool.Enqueue(bullet); // Add the bullet back to the pool
    }

    public void ReturnEXBulletSecondaryToPool(ProjectileBehaviour bullet)
    {
        bullet.gameObject.SetActive(false); // Deactivate the bullet
        EXSecondaryBulletPool.Enqueue(bullet); // Add the bullet back to the pool
    }
}
