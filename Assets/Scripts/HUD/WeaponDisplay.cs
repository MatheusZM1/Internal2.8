using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDisplay : MonoBehaviour
{
    PlayerWeapon weaponScript;

    [Header("Variables")]
    public bool isPlayerTwo;

    [Header("Sprites")]
    public Sprite[] weaponTimerSprites;

    [Header("Icons")]
    public SpriteRenderer arrowIcon;
    public SpriteRenderer primaryIcon;
    public SpriteRenderer secondaryIcon;
    Material primaryMat;
    Material secondaryMat;

    [Header("Values")]
    float primaryIconT;

    private void Awake()
    {
        if (!isPlayerTwo) weaponScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerWeapon>();
        else weaponScript = GameObject.FindGameObjectWithTag("PlayerTwo").GetComponent<PlayerWeapon>();

        primaryMat = primaryIcon.material;
        secondaryMat = secondaryIcon.material;
    }

    private void Start()
    {
        ResetSprites();
    }

    private void OnEnable()
    {
        Actions.levelReset += ResetSprites;
    }

    private void OnDisable()
    {
        Actions.levelReset -= ResetSprites;
    }

    void Update()
    {
        primaryMat.SetFloat("_Arc2", Mathf.Lerp(0, 360, weaponScript.primaryCooldown / weaponScript.EXPrimaryBulletPrefab.weaponStats.fireCooldown));
        secondaryMat.SetFloat("_Arc2", Mathf.Lerp(0, 360, weaponScript.secondaryCooldown / weaponScript.EXSecondaryBulletPrefab.weaponStats.fireCooldown));

        if (!weaponScript.primaryWeaponSelected)
        {
            if (primaryIconT < 1)
            {
                primaryIconT = Mathf.Min(primaryIconT + Time.deltaTime * 3, 1);
                InterpolateWeaponIcons();
            }
        }
        else
        {
            if (primaryIconT > 0)
            {
                primaryIconT = Mathf.Max(primaryIconT - Time.deltaTime * 3, 0);
                InterpolateWeaponIcons();
            }
        }

        if (!isPlayerTwo) arrowIcon.transform.localPosition = Vector2.Lerp(new Vector2(-0.25f, 0.375f), new Vector2(-0.25f, 0.46875f), Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time * 2, 1)));
        else arrowIcon.transform.localPosition = Vector2.Lerp(new Vector2(0.25f, 0.375f), new Vector2(0.25f, 0.46875f), Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time * 2, 1)));

    }

    void InterpolateWeaponIcons()
    {
        if (!isPlayerTwo)
        {
            primaryIcon.transform.localPosition = Vector2.Lerp(new Vector2(-0.25f, 0f), new Vector2(0.25f, 0f), MathFunctions.EaseInOut(primaryIconT, 2));
            secondaryIcon.transform.localPosition = Vector2.Lerp(new Vector2(0.25f, 0f), new Vector2(-0.25f, 0f), MathFunctions.EaseInOut(primaryIconT, 2));
        }
        else
        {
            primaryIcon.transform.localPosition = Vector2.Lerp(new Vector2(0.25f, 0f), new Vector2(-0.25f, 0f), MathFunctions.EaseInOut(primaryIconT, 2));
            secondaryIcon.transform.localPosition = Vector2.Lerp(new Vector2(-0.25f, 0f), new Vector2(0.25f, 0f), MathFunctions.EaseInOut(primaryIconT, 2));
        }
    }

    private void ResetSprites()
    {
        if (!isPlayerTwo)
        {
            primaryIcon.sprite = weaponTimerSprites[LoadoutManager.instance.primaryP1];
            secondaryIcon.sprite = weaponTimerSprites[LoadoutManager.instance.secondaryP1];
        }
        else
        {
            primaryIcon.sprite = weaponTimerSprites[LoadoutManager.instance.primaryP2];
            secondaryIcon.sprite = weaponTimerSprites[LoadoutManager.instance.secondaryP2];
        }
    }
}
