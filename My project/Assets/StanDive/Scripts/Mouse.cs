using UnityEngine;
using UnityEngine.InputSystem;

public class MouseBehaviourScript : MonoBehaviour
{
    float xRot;
    float yRot;

            // 可動域
    [SerializeField]
    float maxUpAngle = 60f;

    [SerializeField]
    float maxDownAngle = 70f;

    [SerializeField]
    float maxHorizontalAngle = 80f;

    void Start()
    {
        //カーソルを画面中央に固定して非表示にする
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //マウスが接続されているかチェック
        if (Mouse.current == null) return;

        float v = Mouse.current.delta.x.ReadValue() * 0.1f;//横移動
        float h = Mouse.current.delta.y.ReadValue() * 0.1f;//縦移動

        xRot -= h;
        yRot += v;

        // 上下制限
        xRot = Mathf.Clamp(xRot, -maxUpAngle, maxDownAngle);

        // 左右制限
        yRot = Mathf.Clamp(yRot, -maxHorizontalAngle, maxHorizontalAngle);

        transform.rotation = Quaternion.Euler(xRot, yRot, 0);
    }
}