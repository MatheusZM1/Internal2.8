using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class WeaponStats : ScriptableObject
{
    [Header("Basic Stats")]
    public float damage;
    public float fireCooldown;
    public float projectileSpeed;

    [Header("Pierce")]
    public bool isPiercing;
    public float pierceCooldown;

    [Header("Projectiles")]
    public int projectileCount;
    public float range;

    [Header("Positioning")]
    public Vector2[] offsetList;
    public float[] angleVarianceList;

    [Header("Instance")]
    public int instanceCount;
}
