using UnityEngine;

public class RopeAttacher : MonoBehaviour
{
    // インスペクターで手のオブジェクトを登録
    public Transform targetHand;

    // 【調整ポイント】手首と手のひらのズレを考慮して、20cm（0.2f）に広げました
    public float attachDistance = 0.2f;

    private bool isAttached = false;

    void Update()
    {
        if (isAttached)
        {
            // 合体後は手の位置に完全同期（角度は真上のまま固定）
            transform.position = targetHand.position;
            transform.rotation = Quaternion.identity;
            return; // 合体済みならこれ以降の処理（距離計算）はスキップ
        }

        // 手の「中心点」とキューブの距離を計算
        float distance = Vector3.Distance(transform.position, targetHand.position);

        // 範囲内に入ったら一瞬で合体
        if (distance <= attachDistance)
        {
            isAttached = true;
        }
    }
}