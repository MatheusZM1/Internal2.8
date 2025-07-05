using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutMenu : MonoBehaviour
{

    InputScript inputInstance;

    public static int loadoutsOpen;

    [Header("Variables")]
    public bool isPlayerTwo;
    public bool isOpen;
    public bool isFinished;

    [Header("Menus")]
    public GameObject masterContainer;
    public GameObject menuContainer;
    public GameObject weaponsContainer;
    public GameObject relicsContainer;

    [Header("Selection Info")]
    public int activeMenu;
    public int menuSelected;
    public int itemSelected;
    public SpriteRenderer selectedSprite, equippedSprite;

    [Header("Input")]
    public float horizontal;
    public float vertical;
    public float movedHorizontalCooldown;
    public float movedVerticalCooldown;

    [Header("Menu")]
    public SpriteRenderer[] menuIcons;

    [Header("Weapons")]
    public Sprite[] weaponIconSprites;
    public SpriteRenderer[] weaponIcons;

    [Header("Relics")]
    public Sprite[] relicIconSprites;
    public SpriteRenderer[] relicIcons;

    private void OnEnable()
    {
        Actions.onLoadout += OpenLoadout;
    }

    private void OnDisable()
    {
        Actions.onLoadout -= OpenLoadout;
    }

    private void Update()
    {
        if (isOpen) // While loadout menu is open
        {
            if (movedHorizontalCooldown > 0) movedHorizontalCooldown -= Time.unscaledDeltaTime;
            if (movedVerticalCooldown > 0) movedVerticalCooldown -= Time.unscaledDeltaTime;

            if (isPlayerTwo) // Retrieve input from correct player
            {
                inputInstance = InputScript.instanceP2;
                if (inputInstance == null)
                {
                    CloseLoadout();
                    return;
                }
            }
            else
            {
                inputInstance = InputScript.instanceP1;
            }

            horizontal = inputInstance.GetHorizontal();
            vertical = inputInstance.GetVertical();

            if (horizontal == 0) movedHorizontalCooldown = 0f;
            if (vertical == 0) movedVerticalCooldown = 0f;

            switch (activeMenu) // Update currently open menu
            {
                case 0:
                    UpdateMenu();
                    break;

                case 1:
                    UpdateWeapons(0);
                    break;

                case 2:
                    UpdateWeapons(1);
                    break;

                case 3:
                    UpdateRelics();
                    break;
            }
        }
    }

    void UpdateMenuIcons()
    {
        if (!isPlayerTwo)
        {
            menuIcons[0].sprite = weaponIconSprites[LoadoutManager.instance.primaryP1];
            menuIcons[1].sprite = weaponIconSprites[LoadoutManager.instance.secondaryP1];
            //menuIcons[2].sprite = relicIconSprites[0];
        }
        else
        {
            menuIcons[0].sprite = weaponIconSprites[LoadoutManager.instance.primaryP2];
            menuIcons[1].sprite = weaponIconSprites[LoadoutManager.instance.secondaryP2];
            //menuIcons[2].sprite = relicIconSprites[0];
        }
        equippedSprite.enabled = false;
    }

    void UpdateMenu()
    {
        if (movedHorizontalCooldown <= 0)
        {
            if (horizontal > 0)
            {
                menuSelected = Mathf.Clamp(menuSelected + 1, 0, 2);
                movedHorizontalCooldown = 0.25f;
            }
            else if (horizontal < 0)
            {
                menuSelected = Mathf.Clamp(menuSelected - 1, 0, 2);
                movedHorizontalCooldown = 0.25f;
            }
        }
        selectedSprite.transform.position = menuIcons[menuSelected].transform.position;

        if (inputInstance.backDown) // Close loadout menu
        {
            CloseLoadout();
            return;
        }

        if (inputInstance.selectDown) // Open selected menu
        {
            switch (menuSelected)
            {
                case 0:
                    OpenMenu(1);
                    break;

                case 1:
                    OpenMenu(2);
                    break;

                case 2:
                    OpenMenu(3);
                    break;
            }
        }
    }

    void UpdateWeapons(int weaponIndex)
    {
        if (movedHorizontalCooldown <= 0)
        {
            if (horizontal > 0)
            {
                itemSelected = Mathf.Clamp(itemSelected + 1, 0, 6);
                movedHorizontalCooldown = 0.25f;
            }
            else if(horizontal < 0)
            {
                itemSelected = Mathf.Clamp(itemSelected - 1, 0, 6);
                movedHorizontalCooldown = 0.25f;
            }
        }

        if (movedVerticalCooldown <= 0)
        {
            if (vertical > 0 && itemSelected > 3)
            {
                itemSelected = Mathf.Clamp(itemSelected - 4, 0, 6);
                movedVerticalCooldown = 0.25f;
            }
            else if (vertical < 0 && itemSelected < 4)
            {
                itemSelected = Mathf.Clamp(itemSelected + 4, 0, 6);
                movedVerticalCooldown = 0.25f;
            }
        }

        selectedSprite.transform.position = weaponIcons[itemSelected].transform.position;

        if (inputInstance.backDown) // Return to menu
        {
            OpenMenu(0);
            return;
        }

        if (inputInstance.selectDown)
        {
            if (weaponIndex == 0) // Change selected primary weapon
            {
                if (!isPlayerTwo) LoadoutManager.instance.SetSelectedPrimaryP1(Mathf.Min(itemSelected, 2)); // Temp cap as not all weapons are implemented
                else LoadoutManager.instance.SetSelectedPrimaryP2(Mathf.Min(itemSelected, 2));
            }
            else // Change selected secondary weapon
            {
                if (!isPlayerTwo) LoadoutManager.instance.SetSelectedSecondaryP1(Mathf.Min(itemSelected, 2));
                else LoadoutManager.instance.SetSelectedSecondaryP2(Mathf.Min(itemSelected, 2));
            }
            equippedSprite.transform.position = weaponIcons[itemSelected].transform.position;
        }
    }

    void UpdateRelics()
    {
        if (movedHorizontalCooldown <= 0)
        {
            if (horizontal > 0)
            {
                itemSelected = Mathf.Clamp(itemSelected + 1, 0, 0);
                movedHorizontalCooldown = 0.25f;
            }
            else if (horizontal < 0)
            {
                itemSelected = Mathf.Clamp(itemSelected - 1, 0, 0);
                movedHorizontalCooldown = 0.25f;
            }
        }

        selectedSprite.transform.position = relicIcons[itemSelected].transform.position;

        if (inputInstance.backDown) // Return to menu
        {
            OpenMenu(0);
            return;
        }


    }

    void OpenLoadout()
    {
        if (isPlayerTwo && !GameManagerScript.instance.playerTwoExists) return;

        masterContainer.SetActive(true);
        OpenMenu(0);
        menuSelected = 0;
        GameManagerScript.instance.loadoutOpen = true;

        loadoutsOpen++;
    }

    void CloseLoadout()
    {
        StartCoroutine(CloseLoadoutCoroutine());
    }

    IEnumerator CloseLoadoutCoroutine()
    {
        yield return null;

        masterContainer.SetActive(false);
        isOpen = false;

        loadoutsOpen--;
        if (loadoutsOpen == 0) GameManagerScript.instance.loadoutOpen = false;
    }


    void OpenMenu(int menuIndex)
    {
        StartCoroutine(OpenMenuCoroutine(menuIndex));
    }

    IEnumerator OpenMenuCoroutine(int menuIndex)
    {
        yield return null;

        menuContainer.SetActive(false);
        weaponsContainer.SetActive(false);
        relicsContainer.SetActive(false);

        equippedSprite.enabled = true;

        switch (menuIndex) // Open selected menu and update relevant items
        {
            case 0:
                menuContainer.SetActive(true);
                isOpen = true;
                UpdateMenuIcons();
                break;

            case 1:
                weaponsContainer.SetActive(true);
                if (!isPlayerTwo) itemSelected = LoadoutManager.instance.primaryP1;
                else itemSelected = LoadoutManager.instance.primaryP2;
                equippedSprite.transform.position = weaponIcons[itemSelected].transform.position;
                break;

            case 2:
                weaponsContainer.SetActive(true);
                if (!isPlayerTwo) itemSelected = LoadoutManager.instance.secondaryP1;
                else itemSelected = LoadoutManager.instance.secondaryP2;
                equippedSprite.transform.position = weaponIcons[itemSelected].transform.position;
                break;

            case 3:
                relicsContainer.SetActive(true);
                break;
        }

        activeMenu = menuIndex;
    }
}
