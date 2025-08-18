using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenus : MonoBehaviour
{
    [Header("Selection Info")]
    public int rowSelected;
    public float movedSelectionCooldown;

    [Header("Pause")]
    public GameObject pauseContainer;
    public SpriteFontMesh[] pauseText;

    [Header("Dead")]
    public GameObject deadContainer;
    public SpriteFontMesh[] deadText;

    private void OnEnable()
    {
        Actions.onGamePause += PauseMenu;
        Actions.onGameDeath += OpenDeathMenu;
    }

    private void OnDisable()
    {
        Actions.onGamePause -= PauseMenu;
        Actions.onGameDeath -= OpenDeathMenu;
    }

    private void Update()
    {
        if (GameManagerScript.instance.gamePaused && !GameManagerScript.instance.loadoutOpen)
        {
            if (GameManagerScript.instance.backDown || GameManagerScript.instance.pauseDown)
            {
                GameManagerScript.instance.PauseGame();
                return;
            }

            HandleInput(pauseText.Length - 1);

            for (int i = 0; i < pauseText.Length; i++) // Highlight selected row
            {
                if (i == rowSelected) pauseText[i].SetColor(0, new Color(236f / 255f, 39f / 255f, 63f / 255f));
                else pauseText[i].SetColor(0, new Color(1, 1, 1));
            }

            if (GameManagerScript.instance.selectDown)
            {
                switch (rowSelected)
                {
                    case 0: // Resume
                        GameManagerScript.instance.PauseGame();
                        break;

                    case 1: // Retry
                        GameManagerScript.instance.PauseGame();
                        Actions.levelReset?.Invoke();
                        break;

                    case 2: // Retry Loadout
                        Actions.onLoadout?.Invoke();
                        break;

                    case 3: // Change scene
                        GameManagerScript.instance.UnpauseGameInstant();
                        Actions.levelReset?.Invoke();
                        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
                        int nextSceneIndex = (currentSceneIndex == 0) ? 1 : 0;
                        SceneManager.LoadScene(nextSceneIndex);
                        break;
                }
            }
        }
        else if (GameManagerScript.instance.gameDead && !GameManagerScript.instance.loadoutOpen)
        {
            HandleInput(deadText.Length - 1);

            for (int i = 0; i < deadText.Length; i++) // Highlight selected row
            {
                if (i == rowSelected) deadText[i].SetColor(0, new Color(236f / 255f, 39f / 255f, 63f / 255f));
                else deadText[i].SetColor(0, new Color(1, 1, 1));
            }

            if (GameManagerScript.instance.selectDown)
            {
                switch (rowSelected)
                {
                    case 0: // Retry
                        Actions.levelReset?.Invoke();
                        CloseDeathMenu();
                        break;

                    case 1: // Loadout
                        Actions.onLoadout?.Invoke();
                        break;

                    case 2: // Change scene
                        Actions.levelReset?.Invoke();
                        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
                        int nextSceneIndex = (currentSceneIndex == 0) ? 1 : 0;
                        SceneManager.LoadScene(nextSceneIndex);
                        break;
                }
            }
        }
    }

    void HandleInput(int maxNumberOfRows)
    {
        if (movedSelectionCooldown > 0) movedSelectionCooldown -= Time.unscaledDeltaTime;

        float vertical = GameManagerScript.instance.vertical;

        if (vertical == 0)
        {
            movedSelectionCooldown = 0f;
        }
        else if (movedSelectionCooldown <= 0)
        {
            if (vertical > 0)
            {
                rowSelected--;
                movedSelectionCooldown = 0.25f;
            }
            else
            {
                rowSelected++;
                movedSelectionCooldown = 0.25f;
            }
            rowSelected = Mathf.Clamp(rowSelected, 0, maxNumberOfRows); // Clamp selected row
        }
    }

    void PauseMenu(bool doPause)
    {
        if (doPause)
        {
            pauseContainer.SetActive(true);
            rowSelected = 0;
            for (int i = 0; i < pauseText.Length; i++) // Highlight first row
            {
                if (i == 0) pauseText[i].SetColor(0, new Color(236f / 255f, 39f / 255f, 63f / 255f));
                else pauseText[i].SetColor(0, new Color(1, 1, 1));
            }
        }
        else
        {
            pauseContainer.SetActive(false);
        }
    }

    void OpenDeathMenu()
    {
        deadContainer.SetActive(true);
        GameManagerScript.instance.gameDead = true;
        if (GameManagerScript.instance.gamePaused) GameManagerScript.instance.PauseGame();

        rowSelected = 0;
        for (int i = 0; i < deadText.Length; i++) // Highlight first row
        {
            if (i == 0) deadText[i].SetColor(0, new Color(236f / 255f, 39f / 255f, 63f / 255f));
            else deadText[i].SetColor(0, new Color(1, 1, 1));
        }
    }

    void CloseDeathMenu()
    {
        deadContainer.SetActive(false);
    }
}
