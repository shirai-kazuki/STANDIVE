using UnityEngine;

public class RotateY : MonoBehaviour
{
    // 回転速度
    public float speed = 1000f;

    void Update()
    {
        // Y軸を中心に毎秒 speed 度だけ回転させる
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
