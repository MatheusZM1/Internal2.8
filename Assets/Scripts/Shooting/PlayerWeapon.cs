using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public PlayerMovement playerScript;

    [Header("Player Weapon")]
    public WeaponStats mainWeapon;
    public WeaponStats secondaryWeapon;
    public ProjectileBehaviour bulletPrefab;

    [Header("Shooting")]
    public float fireCooldown;

    private void Awake()
    {
        playerScript = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (fireCooldown > 0) fireCooldown -= Time.deltaTime;
        if (Input.GetKey("d") && fireCooldown <= 0)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        ProjectileBehaviour newBullet = Instantiate(bulletPrefab);
        newBullet.transform.position = transform.position;
        if (playerScript.directionFacing.y == 0) newBullet.SetVelocity(playerScript.directionFacing);
        else
        {
            if (playerScript.horizontal == 0) newBullet.SetVelocity(new Vector2(0, playerScript.directionFacing.y));
            else newBullet.SetVelocity(new Vector2(playerScript.horizontal, playerScript.directionFacing.y));
        }
        fireCooldown = newBullet.weaponStats.fireCooldown;
    }
}
