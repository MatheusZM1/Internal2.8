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

    [Header("Shooting")]
    public bool primaryWeaponSelected;
    public float fireCooldown;
    public int primaryOffsetIndex;
    public int secondaryOffsetIndex;

    private Queue<ProjectileBehaviour> primaryBulletPool = new Queue<ProjectileBehaviour>();
    private Queue<ProjectileBehaviour> secondaryBulletPool = new Queue<ProjectileBehaviour>();

    private void Awake()
    {
        playerScript = GetComponent<PlayerMovement>();
        inputScript = GetComponent<InputScript>();
        primaryWeaponSelected = true;

        // Initialize the bullet pool
        for (int i = 0; i < primaryBulletPrefab.weaponStats.instanceCount; i++)
        {
            ProjectileBehaviour bullet = Instantiate(primaryBulletPrefab);
            bullet.isPrimary = true;
            bullet.playerWeapon = this;

            bullet.gameObject.SetActive(false); // Deactivate initially
            primaryBulletPool.Enqueue(bullet); // Add to pool
        }

        for (int i = 0; i < secondaryBulletPrefab.weaponStats.instanceCount; i++)
        {
            ProjectileBehaviour bullet = Instantiate(secondaryBulletPrefab);
            bullet.isPrimary = false;
            bullet.playerWeapon = this;

            bullet.gameObject.SetActive(false); // Deactivate initially
            secondaryBulletPool.Enqueue(bullet); // Add to pool
        }
    }

    private void Update()
    {
        if (fireCooldown > 0) fireCooldown -= Time.deltaTime;
        if ((Input.GetKey("a") && !playerScript.isPlayerTwo) || inputScript.GetActionHold("RTrigger"))
        {
            if (fireCooldown <= 0) Shoot();
        }
        if ((Input.GetKeyDown("s") && !playerScript.isPlayerTwo) || inputScript.GetActionDown("LBumper")) primaryWeaponSelected = !primaryWeaponSelected;
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
                newBullet.SetVelocity(playerScript.directionFacing, offsetIndex);
            }
            else
            {
                if (playerScript.horizontal == 0)
                {
                    newBullet.SetVelocity(new Vector2(0, playerScript.directionFacing.y), offsetIndex);
                }
                else
                {
                    newBullet.SetVelocity(new Vector2(playerScript.horizontal, playerScript.directionFacing.y), offsetIndex);
                }
            }

            // Activate the bullet
            newBullet.gameObject.SetActive(true);

            // Set the fire cooldown
            fireCooldown = newBullet.weaponStats.fireCooldown;

            bulletPoolCount = primaryWeaponSelected ? primaryBulletPool.Count : secondaryBulletPool.Count;
        }
    }

    public void ReturnBulletToPrimaryPool(ProjectileBehaviour bullet)
    {
        bullet.gameObject.SetActive(false);  // Deactivate the bullet
        primaryBulletPool.Enqueue(bullet);          // Add the bullet back to the pool
    }

    public void ReturnBulletSecondaryToPool(ProjectileBehaviour bullet)
    {
        bullet.gameObject.SetActive(false);  // Deactivate the bullet
        secondaryBulletPool.Enqueue(bullet);          // Add the bullet back to the pool
    }
}
