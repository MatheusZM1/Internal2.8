using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class WeaponStats : ScriptableObject
{
    [Header("Basic Stats")]
    public float damage;
    public float fireCooldown;
    public float projectileSpeed;
    public bool isPiercing;

    [Header("Projectiles")]
    public int projectileCount;
    public float range;

    [Header("Positioning")]
    public Vector2[] offsetList;
    public float[] angleVarianceList;

    [Header("Instance")]
    public int instanceCount;
}
