using UnityEngine;

public class RingNPCmanager : MonoBehaviour
{
    public Transform target; // 目的地のTransformを保存する変数
    private float speed = 10f;

    void Start()
    {
        
    }

    void Update()
    {
        if (target != null)
        {
            // ターゲットの座標を計算
            Vector3 targetPosition = target.position;

            // 移動処理
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        }
    }
}
