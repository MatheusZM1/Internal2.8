using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class WeaponStats : ScriptableObject
{
    [Header("Basic Stats")]
    public float damage;
    public float fireCooldown;
    public float projectileSpeed;
}
