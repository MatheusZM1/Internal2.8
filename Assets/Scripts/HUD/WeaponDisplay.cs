using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDisplay : MonoBehaviour
{
    PlayerWeapon weaponScript;

    [Header("Variables")]
    public bool isPlayerTwo;

    [Header("Icons")]
    public SpriteRenderer primaryIcon;
    public SpriteRenderer secondaryIcon;
    Material primaryMat;
    Material secondaryMat;

    private void Awake()
    {
        if (!isPlayerTwo) weaponScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerWeapon>();
        else weaponScript = GameObject.FindGameObjectWithTag("PlayerTwo").GetComponent<PlayerWeapon>();

        primaryMat = primaryIcon.material;
        secondaryMat = secondaryIcon.material;
    }

    void Update()
    {
        primaryMat.SetFloat("_Arc2", Mathf.Lerp(0, 360, weaponScript.primaryCooldown / weaponScript.EXPrimaryBulletPrefab.weaponStats.fireCooldown));
        secondaryMat.SetFloat("_Arc2", Mathf.Lerp(0, 360, weaponScript.secondaryCooldown / weaponScript.EXSecondaryBulletPrefab.weaponStats.fireCooldown));
    }
}
