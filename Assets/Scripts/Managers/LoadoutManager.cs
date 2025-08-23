using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager instance;

    [Header("Weapon Presets")]
    public ProjectileBehaviour[] normalBullets;
    public ProjectileBehaviour[] EXBullets;

    [Header("Player Selection Info")]
    public int primaryP1;
    public int secondaryP1;
    public int buffP1;
    public int primaryP2;
    public int secondaryP2;
    public int buffP2;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        primaryP1 = 0;
        secondaryP1 = 1;
        buffP1 = 0;
        primaryP2 = 0;
        secondaryP2 = 1;
        buffP2 = 0;
    }

    public void SetSelectedPrimaryP1(int newSelection)
    {
        if (secondaryP1 == newSelection) secondaryP1 = primaryP1; // Swap weapons to prevent double ups
        primaryP1 = newSelection;
    }

    public void SetSelectedSecondaryP1(int newSelection)
    {
        if (primaryP1 == newSelection) primaryP1 = secondaryP1; // Swap weapons to prevent double ups
        secondaryP1 = newSelection;
    }

    public void SetSelectedBuffP1(int newSelection)
    {
        buffP1 = newSelection;
    }

    public void SetSelectedPrimaryP2(int newSelection)
    {
        if (secondaryP2 == newSelection) secondaryP2 = primaryP2; // Swap weapons to prevent double ups
        primaryP2 = newSelection;
    }

    public void SetSelectedSecondaryP2(int newSelection)
    {
        if (primaryP2 == newSelection) primaryP2 = secondaryP2; // Swap weapons to prevent double ups
        secondaryP2 = newSelection;
    }

    public void SetSelectedBuffP2(int newSelection)
    {
        buffP2 = newSelection;
    }

    public ProjectileBehaviour GetPrimaryNormal(bool isPlayerTwo)
    {
        if (isPlayerTwo)
        {
            return normalBullets[primaryP2];
        }
        else
        {
            return normalBullets[primaryP1];
        }
    }

    public ProjectileBehaviour GetPrimaryEX(bool isPlayerTwo)
    {
        if (isPlayerTwo)
        {
            return EXBullets[primaryP2];
        }
        else
        {
            return EXBullets[primaryP1];
        }
    }

    public ProjectileBehaviour GetSecondaryNormal(bool isPlayerTwo)
    {
        if (isPlayerTwo)
        {
            return normalBullets[secondaryP2];
        }
        else
        {
            return normalBullets[secondaryP1];
        }
    }

    public ProjectileBehaviour GetSecondaryEX(bool isPlayerTwo)
    {
        if (isPlayerTwo)
        {
            return EXBullets[secondaryP2];
        }
        else
        {
            return EXBullets[secondaryP1];
        }
    }
}
