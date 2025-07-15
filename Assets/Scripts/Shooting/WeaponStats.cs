using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class WeaponStats : ScriptableObject
{
    [Header("Basic Stats")]
    public float damage;
    public float fireCooldown;
    public float projectileSpeed;

    [Header("Speeds")]
    public float speedVarianceMin;
    public float speedVarianceMax;
    public float playerSpeedModifier;

    [Header("Pierce")]
    public bool isPiercing;
    public float pierceCooldown;

    [Header("Projectiles")]
    public int projectileCount;
    public float range;

    [Header("Positioning")]
    public Vector2[] offsetList;
    public float[] angleVarianceList;
    public float yVarianceMin;
    public float yVarianceMax;

    [Header("Scaling")]
    public float scaleMin = 1;
    public float scaleMax = 1;

    [Header("Instance")]
    public int instanceCount;

    [Header("Visuals")]
    public bool doNotRotateSprite;
}
