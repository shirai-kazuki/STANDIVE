using UnityEngine;

public class RingTargetManager : MonoBehaviour
{
    public Transform target; // 目的地のTransformを保存する変数
    private float vertical;
    private float horizontal;

    void Start()
    {
        vertical = 3520f;
        SetHorizontal(5000f);
    }

    void Update()
    {
        if (target != null)
    {
        // ターゲットの座標を計算
        Vector3 targetPosition = target.position;

        if(targetPosition.y <= transform.position.y && targetPosition.y <= 3500f && targetPosition.y >= 300f)
            {
                vertical = targetPosition.y - 400f;
                SetHorizontal(targetPosition.x);
            }

        // XY座標だけ「自分自身の高さ」に書き換える（ターゲットのYを無視）
        targetPosition.y = vertical;
        targetPosition.x = horizontal;

        // 移動処理
        transform.position = targetPosition;

    }
    }

    public void SetHorizontal(float TargetX)
    {
        horizontal = Random.Range(TargetX - 3f, TargetX + 3f);
    }
}
