using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputScript : MonoBehaviour
{
    private InputActions inputActions;
    public static int activeInputDevice;

    public static InputScript instanceP1;
    public static InputScript instanceP2;

    public bool isPlayerTwo;
    public bool isAnyPlayer;

    [Header("Binds")]
    public string horizontalPositiveKey;
    public string horizontalNegativeKey;
    public string verticalPositiveKey;
    public string verticalNegativeKey;
    public string selectKey;
    public string backKey;

    [Header("UI Input")]
    public bool axisRaw;
    public bool selectDown, selectHold;
    public bool backDown, backHold;

    private HashSet<string> downActions = new HashSet<string>();
    private HashSet<string> upActions = new HashSet<string>();
    private HashSet<string> holdActions = new HashSet<string>();

    private InputUser inputUser;
    private Gamepad assignedGamepad;

    private void Awake()
    {
        inputActions = new InputActions();

        if (!isAnyPlayer)
        {
            if (isPlayerTwo)
            {
                instanceP2 = this;
            }
            else
            {
                instanceP1 = this;
            }
        }
    }

    private void Start()
    {
        AssignController();
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;

        inputActions.Player.Enable();

        RegisterAction(inputActions.Player.SouthButton, "SouthB");
        RegisterAction(inputActions.Player.NorthButton, "NorthB");
        RegisterAction(inputActions.Player.EastButton, "EastB");
        RegisterAction(inputActions.Player.WestButton, "WestB");
        RegisterAction(inputActions.Player.Pause, "Pause");
        RegisterAction(inputActions.Player.RBumper, "RBumper");
        RegisterAction(inputActions.Player.LBumper, "LBumper");
        RegisterAction(inputActions.Player.RTrigger, "RTrigger");
        RegisterAction(inputActions.Player.LTrigger, "LTrigger");
    }
    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            AssignController();
        }
    }

    void AssignController()
    {
        if (isAnyPlayer) return;
        // Get gamepads
        var gamepads = Gamepad.all;

        if (isPlayerTwo && gamepads.Count > 1)
        {
            assignedGamepad = gamepads[1];
        }
        else if (!isPlayerTwo && gamepads.Count > 0)
        {
            assignedGamepad = gamepads[0];
        }

        GameManagerScript.instance.playerTwoExists = gamepads.Count > 1;
        Actions.onPlayerTwoConnect?.Invoke(gamepads.Count > 1);

        if (assignedGamepad != null)
        {
            inputUser = InputUser.PerformPairingWithDevice(assignedGamepad);
            inputUser.AssociateActionsWithUser(inputActions);
        }
    }

    private void Update()
    {
        // Check if any key is pressed on the keyboard
        if (Keyboard.current.anyKey.wasPressedThisFrame && activeInputDevice != 0)
        {
            activeInputDevice = 0;
            //Actions.onActiveInputDeviceUpdate?.Invoke();
        }

        if ((Input.GetKeyDown(selectKey) && !isPlayerTwo) || GetActionDown("SouthB")) selectDown = true; // Select down
        else selectDown = false;
        if ((Input.GetKey(selectKey) && !isPlayerTwo) || GetActionHold("SouthB")) selectHold = true; // Select hold
        else selectHold = false;

        if ((Input.GetKeyDown(backKey) && !isPlayerTwo) || GetActionDown("EastB")) backDown = true; // Back down
        else backDown = false;
        if ((Input.GetKey(backKey) && !isPlayerTwo) || GetActionHold("EastB")) backHold = true; // Back hold
        else backHold = false;
    }

    private void LateUpdate()
    {
        downActions.Clear();
        upActions.Clear();
    }

    private void RegisterAction(InputAction action, string actionName)
    {
        action.started += ctx => {
            if (ctx.control.device != assignedGamepad && !isAnyPlayer) return;

            downActions.Add(actionName);
            holdActions.Add(actionName);

            var device = ctx.control.device;
            if (device != null)
            {
                string deviceName = device.displayName;
                if (deviceName.Contains("Xbox"))
                {
                    if (activeInputDevice != 1)
                    {
                        activeInputDevice = 1;
                        //Actions.onActiveInputDeviceUpdate?.Invoke();
                    }
                }
                else if (deviceName.Contains("PlayStation"))
                {
                    if (activeInputDevice != 2)
                    {
                        activeInputDevice = 2;
                        //Actions.onActiveInputDeviceUpdate?.Invoke();
                    }
                }
                else if (deviceName.Contains("Switch"))
                {
                    if (activeInputDevice != 3)
                    {
                        activeInputDevice = 3;
                        //Actions.onActiveInputDeviceUpdate?.Invoke();
                    }
                }
            }
        };
        action.canceled += ctx => {
            if (ctx.control.device != assignedGamepad && !isAnyPlayer) return;

            upActions.Add(actionName);
            holdActions.Remove(actionName);
        };
    }

    private void UnregisterAction(InputAction action, string actionName)
    {
        action.started -= ctx => {
            if (ctx.control.device != assignedGamepad) return;

            downActions.Add(actionName);
            holdActions.Add(actionName);
        };
        action.canceled -= ctx => {
            if (ctx.control.device != assignedGamepad) return;

            upActions.Add(actionName);
            holdActions.Remove(actionName);
        };
    }

    public bool GetActionDown(string actionName)
    {
        return downActions.Contains(actionName);
    }

    public bool GetActionHold(string actionName)
    {
        return holdActions.Contains(actionName);
    }

    public bool GetActionUp(string actionName)
    {
        return upActions.Contains(actionName);
    }

    float GetRawValue(float value)
    {
        if (value == 0) return 0;
        return Mathf.Sign(value);
    }

    public Vector2 GetPlayerAxis()
    {
        if (assignedGamepad == null && !isAnyPlayer) return Vector2.zero;

        var actionControl = inputActions.Player.Axis.activeControl;

        if (actionControl != null && actionControl.device != assignedGamepad && !isAnyPlayer) return Vector2.zero;

        if (actionControl != null)
        {
            var device = actionControl.device;
            if (device != null)
            {
                if (device is Gamepad gamepad)
                {
                    if (gamepad.displayName.Contains("Xbox"))
                    {
                        if (activeInputDevice != 1)
                        {
                            activeInputDevice = 1;
                            //Actions.onActiveInputDeviceUpdate?.Invoke();
                        }
                    }
                    else if (gamepad.displayName.Contains("PlayStation"))
                    {
                        if (activeInputDevice != 2)
                        {
                            activeInputDevice = 2;
                            //Actions.onActiveInputDeviceUpdate?.Invoke();
                        }
                    }
                    else if (gamepad.displayName.Contains("Switch"))
                    {
                        if (activeInputDevice != 3)
                        {
                            activeInputDevice = 3;
                            //Actions.onActiveInputDeviceUpdate?.Invoke();
                        }
                    }
                }
            }
        }

        Vector2 input = inputActions.Player.Axis.ReadValue<Vector2>();
        input = new Vector2(Mathf.Abs(input.x) > ControlOptions.controllerDeadZone ? input.x : 0, Mathf.Abs(input.y) > ControlOptions.controllerDeadZone ? input.y : 0); // Return joystick input with deadzone limits
        return axisRaw ? new Vector2(GetRawValue(input.x), GetRawValue(input.y)) : input;
    }

    float GetCustomAxis(string positiveKey, string negativeKey)
    {
        float positiveInput = Input.GetKey(positiveKey) ? 1f : 0f;
        float negativeInput = Input.GetKey(negativeKey) ? -1f : 0f;
        return Mathf.Abs(positiveInput + negativeInput) > ControlOptions.controllerDeadZone ? positiveInput + negativeInput : 0;
    }

    public float GetHorizontal()
    {
        float horizontalKey = isPlayerTwo ? 0 : GetCustomAxis(horizontalPositiveKey, horizontalNegativeKey);
        return Mathf.Clamp(horizontalKey + GetPlayerAxis().x, -1, 1);
    }

    public float GetVertical()
    {
        float verticalKey = isPlayerTwo ? 0 : GetCustomAxis(verticalPositiveKey, verticalNegativeKey);
        return Mathf.Clamp(verticalKey + GetPlayerAxis().y, -1, 1);
    }
}