using UnityEngine;

public class RopeAttacher : MonoBehaviour
{
    // インスペクターで「RightHandAnchor」などを指定
    public Transform targetHandAnchor; 
    private bool isAttached = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        // 手のアンカー（またはその子オブジェクト）にぶつかったら
        if (other.transform == targetHandAnchor || other.transform.IsChildOf(targetHandAnchor))
        {
            isAttached = true;

            // 物理挙動がついていれば無効化して動きを止める
            if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }
        }
    }

    void Update()
    {
        if (isAttached)
        {
            // 位置は手のひらの中心にぴったり合わせる
            transform.position = targetHandAnchor.position;
            
            // 角度は世界の真上（回転なし：0, 0, 0）に固定し続ける
            transform.rotation = Quaternion.identity;
        }
    }
}