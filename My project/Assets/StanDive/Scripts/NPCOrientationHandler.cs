using System.Collections.Generic;
using UnityEngine;

public class NPCOrientationHandler : MonoBehaviour
{
    [SerializeField] private List<Transform> NPCs = new List<Transform>();

    private Rigidbody rb;  // PlayerのRigidbodyコンポーネントを保存する変数

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float targetZ = 0f;

        if (rb.linearVelocity.x > 0.01f)
        {
            targetZ = -40f; // 右移動時のZ角度
        }
        else if (rb.linearVelocity.x < -0.01f)
        {
            targetZ = 40f;  // 左移動時のZ角度
        }
        else
        {
            targetZ = 0f;   // 停止時のZ角度
        }

        // リストに入っているすべてのNPCの「SetRoll」にZ角度をセットする
        foreach (Transform npc in NPCs)
        {
            if (npc == null) continue;

            // NPCについている RingNPCmanager を取得して関数を呼び出す
            RingNPCmanager manager = npc.GetComponent<RingNPCmanager>();
            if (manager != null)
            {
                manager.SetRoll(targetZ); // ★ここで横の角度をセット！
            }
        }
    }
}
