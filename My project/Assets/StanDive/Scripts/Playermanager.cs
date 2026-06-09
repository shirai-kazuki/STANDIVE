using UnityEngine;
using UnityEngine.InputSystem;

public class Playermanager : MonoBehaviour
{
    private Rigidbody rb;
    private float moveSpeed = 30f;
    private float height;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //空気抵抗を0.1にする
        rb.linearDamping = 0.1f; 
        //初期の高さを保存
        height = transform.position.y;
    }
    private void FixedUpdate()
    {
        MovePlayer();

        if (transform.position.y < height - 1500f)
        {
            // プレイヤーが一定の高さより下に落ちたら、子オブジェクトをアクティブにする
            ActiveChildByName("Parachute");
            // プレイヤーが一定の高さより下に落ちたら、子オブジェクトを非アクティブにする
            DeactivateChildByName("WindPressure");
            //空気抵抗を0.5にする
            rb.linearDamping = 0.7f;
        }
    }

    //名前を指定して子オブジェクトをアクティブにする関数
    private void ActiveChildByName(string targetName)
    {
        Transform child = transform.Find(targetName);
        if (!child.gameObject.activeSelf)
        {
            child.gameObject.SetActive(true);
        }
    }

    //名前を指定して子オブジェクトを非アクティブにする関数
    private void DeactivateChildByName(string targetName)
    {
        Transform child = transform.Find(targetName);
        if (child.gameObject.activeSelf)
        {
            child.gameObject.SetActive(false);
        }
    }

    // プレイヤーを移動させる関数
    private void MovePlayer()
    {
        // キーボードがPCに認識されているか安全のためにチェック
        if (Keyboard.current == null) return;

        // 前
        if (Keyboard.current.wKey.isPressed)
        {
            rb.AddForce(transform.forward * moveSpeed);
        }

        // 後ろ
        if (Keyboard.current.sKey.isPressed)
        {
            rb.AddForce(-transform.forward * moveSpeed);
        }

        //右に進む
        if (Keyboard.current.dKey.isPressed)
        {
            rb.AddForce(transform.right * moveSpeed);
        }
        //左に進む
        if (Keyboard.current.aKey.isPressed)
        {
            rb.AddForce(-transform.right * moveSpeed);
        }
    }



}
