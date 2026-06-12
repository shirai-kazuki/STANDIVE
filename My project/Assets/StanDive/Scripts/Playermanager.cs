using UnityEngine;
using UnityEngine.InputSystem;

public class Playermanager : MonoBehaviour
{
    private Rigidbody rb;
    private float moveSpeed = 15f;
    private float height;
    // 左手と右手の高さを保存する変数
    public float leftHandHeight;
    public float rightHandHeight;
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

        if (transform.position.y < height - 3000f)
        {
            // プレイヤーが一定の高さより下に落ちたら、子オブジェクトをアクティブにする
            ActiveChildByName("Parachute");
            // プレイヤーが一定の高さより下に落ちたら、子オブジェクトを非アクティブにする
            DeactivateChildByName("WindPressure");
            //空気抵抗を0.5にする
            rb.linearDamping = 0.5f;
        }
        if (transform.position.y < height - 3200f)
        {
            //空気抵抗を1にする
            rb.linearDamping = 1f;
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
        if (leftHandHeight > rightHandHeight + 0.1f) // 左手が右手よりも0.1m以上高い場合
        {
            rb.AddForce(transform.right * moveSpeed);
        }
        //左に進む
        if (rightHandHeight > leftHandHeight + 0.1f) // 右手が左手よりも0.1m以上高い場合
        {
            rb.AddForce(-transform.right * moveSpeed);
        }
    }

    // 左手用の高さをセットするための関数
    public void SetLeftHeight(float height)
    {
        leftHandHeight = height;
    }

    // 右手用の高さをセットするための関数
    public void SetRightHeight(float height)
    {
        rightHandHeight = height;
    }

}
