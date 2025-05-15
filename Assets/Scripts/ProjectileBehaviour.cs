using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public enum PlayerWeapon
    {
        normal
    }

    [Header("Stats")]
    public WeaponStats weaponStats;
    public Vector2 velocity;

    private void FixedUpdate()
    {
        transform.position += (Vector3)velocity * Time.fixedDeltaTime;
    }

    public void SetVelocity(Vector2 direction)
    {
        velocity = direction.normalized * weaponStats.projectileSpeed;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
    }
}
