using UnityEngine;
using UnityEngine.InputSystem;

public class Playermanager : MonoBehaviour
{
    private Rigidbody rb;
    private float moveSpeed = 30f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        // キーボードがPCに認識されているか安全のためにチェック
        if (Keyboard.current == null) return;

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
