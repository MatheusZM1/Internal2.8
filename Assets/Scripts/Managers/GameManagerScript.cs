using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance;

    [Header("Input")]
    public float horizontal;
    public float vertical;
    public bool selectDown, selectHold;
    public bool backDown, backHold;
    public bool pauseDown, pauseHold;

    [Header("Binds")]
    public string horizontalPositiveKey;
    public string horizontalNegativeKey;
    public string verticalPositiveKey;
    public string verticalNegativeKey;
    public string selectKey;
    public string backKey;
    public string pauseKey;
    InputScript inputScript;

    [Header("Player Info")]
    public bool playerTwoExists;

    [Header("Game Info")]
    public bool gamePaused;
    public bool gameDead;
    public bool loadoutOpen;

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

        inputScript = GetComponent<InputScript>();
    }

    private void OnEnable()
    {
        Actions.levelReset += ResetLevel;
    }

    float GetCustomAxis(string positiveKey, string negativeKey)
    {
        float positiveInput = Input.GetKey(positiveKey) ? 1f : 0f;
        float negativeInput = Input.GetKey(negativeKey) ? -1f : 0f;
        return Mathf.Abs(positiveInput + negativeInput) > ControlOptions.controllerDeadZone ? positiveInput + negativeInput : 0;
    }

    private float GetHorizontal()
    {
        float horizontalKey = GetCustomAxis(horizontalPositiveKey, horizontalNegativeKey);
        return Mathf.Clamp(horizontalKey + inputScript.GetPlayerAxis().x, -1, 1);
    }

    private float GetVertical()
    {
        float verticalKey = GetCustomAxis(verticalPositiveKey, verticalNegativeKey);
        return Mathf.Clamp(verticalKey + inputScript.GetPlayerAxis().y, -1, 1);
    }

    private void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        horizontal = GetHorizontal();
        vertical = GetVertical();

        if (Input.GetKeyDown(selectKey) || inputScript.GetActionDown("SouthB")) selectDown = true; // Select down
        else selectDown = false;
        if (Input.GetKey(selectKey) || inputScript.GetActionHold("SouthB")) selectHold = true; // Select hold
        else selectHold = false;

        if (Input.GetKeyDown(backKey) || inputScript.GetActionDown("EastB")) backDown = true; // Back down
        else backDown = false;
        if (Input.GetKey(backKey) || inputScript.GetActionHold("EastB")) backHold = true; // Back hold
        else backHold = false;

        if (Input.GetKeyDown(pauseKey) || inputScript.GetActionDown("Pause")) pauseDown = true; // Select down
        else pauseDown = false;
        if (Input.GetKey(pauseKey) || inputScript.GetActionHold("Pause")) pauseHold = true; // Select hold
        else pauseHold = false;
    }

    void ResetLevel()
    {
        gameDead = false;
        PlayerMovement.playersDead = 0;
    }

    public void PauseGame()
    {
        StartCoroutine(PauseCoroutine());
    }

    IEnumerator PauseCoroutine()
    {
        yield return null;
        gamePaused = !gamePaused;

        if (gamePaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        Actions.onGamePause?.Invoke(gamePaused);
    }
}
