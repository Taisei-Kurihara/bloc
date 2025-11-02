using UnityEngine;
using UnityEngine.InputSystem;

public class TestInput_check : MonoBehaviour
{
    // (既)修: InputAction の一覧とその対応入力を取得して順にlogを出す関数を追加してstartで呼び出し.
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //LogAllInputActions();
    }

    // Update is called once per frame
    void Update()
    {
        //LogCurrentInput();
    }

    // InputActionの一覧とその対応入力を取得して順にlogを出す関数.
    void LogAllInputActions()
    {
        Debug.Log("=== InputAction 一覧 ===");

        // すべてのInputActionAssetを取得.
        var allActions = InputSystem.ListEnabledActions();

        foreach (var action in allActions)
        {
            Debug.Log($"Action名: {action.name}, バインディング: {action.bindings.Count}個");

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                Debug.Log($"  - バインディング{i}: {binding.effectivePath}");
            }
        }
    }

    // 現在の入力をlogで返す機能.
    void LogCurrentInput()
    {
        // キーボード入力をチェック.
        if (Keyboard.current != null)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame)
                {
                    Debug.Log($"入力: {key.name}");
                }
            }
        }

        // マウス入力をチェック.
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log("入力: LeftMouseButton");
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Debug.Log("入力: RightMouseButton");
            }
            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                Debug.Log("入力: MiddleMouseButton");
            }
        }

        // ゲームパッド入力をチェック.
        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is UnityEngine.InputSystem.Controls.ButtonControl button)
                {
                    if (button.wasPressedThisFrame)
                    {
                        Debug.Log($"入力: {button.name}");
                    }
                }
            }
        }
    }
}
