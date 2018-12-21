/*

        InputManager.Instance.OnControllerButtonEvent.AddListener(delegate (ButtonType type) {
            print("OnUnityEvent.AddListener: " + type.ToString());
        });

 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public enum ButtonType
{
    A,
    B,
    X,
    Y,
    L,
    R
}

public enum ActionType
{
    Jump,
    Attack,
    Dash,
    Avoid
}

[System.Serializable]
public class ActionEvent : UnityEvent<ActionType> {};

[System.Serializable]
public class ButtonEvent : UnityEvent<ButtonType> {};

public class InputManager : SingletonMonoBehaviour<InputManager>
{
    //Cinemachineカメラ
    [SerializeField] CinemachineFreeLook freeLookCamera;

    //ボタンイベントの発行
    [SerializeField] public ButtonEvent OnButtonEvent;
    [SerializeField] public ActionEvent OnActionEvent;

    private void Start()
    {
        // FreelookCameraの設定
        SetFreelookCamera();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") || Input.GetButtonDown(ButtonType.Y.ToString()))
        {
            if (OnActionEvent != null)
            {
                OnActionEvent.Invoke(ActionType.Jump);
            }
        }

        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown(ButtonType.R.ToString()))
        {
            if (OnActionEvent != null)
            {
                OnActionEvent.Invoke(ActionType.Attack);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown(ButtonType.A.ToString()))
        {
            if (OnActionEvent != null)
            {
                OnActionEvent.Invoke(ActionType.Avoid);
            }
        }
    }

    //FreelookCameraの操作をマウスではなくコントローラーの右のキノコを使用する
    private void SetFreelookCamera()
    {
        if (!freeLookCamera) return;

        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        freeLookCamera.m_XAxis.m_InputAxisName = "Xbox Right Stick Horizontal Win";
        freeLookCamera.m_YAxis.m_InputAxisName = "Xbox Right Stick Vertical Win";

        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        freeLookCamera.m_XAxis.m_InputAxisName = "Xbox Right Stick Horizontal Mac";
        freeLookCamera.m_YAxis.m_InputAxisName = "Xbox Right Stick Vertical Mac";

        #endif
    }
}