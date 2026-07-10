using UnityEngine;
using UnityEngine.InputSystem;

public class RingNPCmanager : MonoBehaviour
{
    public Transform target; // 目的地のTransformを保存する変数
    private float speed = 50f;
    private float arrivalThreshold = 0f; // 到達とみなす距離
    private Quaternion startRotation;
    // 目標の角度
    private Quaternion targetRotation;
    // 回転するスピード
    private float rotationSpeed = 30f; 
    private Vector3 targetPosition;

    void Start()
    {
        // ゲーム開始時の初期姿勢を記憶しておく
        startRotation = transform.rotation;
    }

    void Update()
    {
        if (target != null)
        {
            // ターゲットの座標を計算
            Vector3 targetPosition = target.position;

            // 上に消えるためのに速度を上げる
            if(transform.position.y < 300f)
            {
                speed =25f;
            }

            // 移動処理
            if(target.position.y < 3400f)
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            }

            // 現在地と目的地の距離を計算
            float distance = transform.position.y - target.position.y;

            // 距離が閾値以下になったら到着とする
            if (distance <= arrivalThreshold)
            {
                // 初期姿勢
                targetRotation = startRotation;
                speed = 2f;
            }
            else
            {
                // 初期姿勢からY軸だけ90度傾いた状態
                targetRotation = startRotation * Quaternion.Euler(90, 0, 0);
            }

            // 現在の角度から、目標の角度へ「徐々に」近づける
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                Time.deltaTime * rotationSpeed
            );
        }
    }
}
