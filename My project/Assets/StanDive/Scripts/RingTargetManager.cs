using UnityEngine;

public class RingTargetManager : MonoBehaviour
{
    public Transform target; // 目的地のTransformを保存する変数
    public float speed = 500f;
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

        if(targetPosition.y <= transform.position.y && targetPosition.y <= 3500f && targetPosition.y >= 500f)
            {
                vertical = targetPosition.y - 200f;
                SetHorizontal(targetPosition.x);
            }

        // XY座標だけ「自分自身の高さ」に書き換える（ターゲットのYを無視）
        targetPosition.y = vertical;
        targetPosition.x = horizontal;

        // 移動処理
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

    }
    }

    public void SetHorizontal(float TargetX)
    {
        horizontal = Random.Range(TargetX - 50f, TargetX + 50f);
    }
}
