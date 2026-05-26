using UnityEngine;
using UnityEngine.InputSystem;

public class MouseBehaviourScript : MonoBehaviour
{
    float xRot;
    float yRot;

    void Start()
    {
        //カーソルを画面中央に固定して非表示にする
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //マウスが接続されているかチェック
        if (Mouse.current == null) return;

        float h = Mouse.current.delta.x.ReadValue() * 0.1f;//横移動
        float v = Mouse.current.delta.y.ReadValue() * 0.1f;//縦移動

        xRot += v;
        yRot += h;
        transform.rotation = Quaternion.Euler(xRot, yRot, 0);
    }
}