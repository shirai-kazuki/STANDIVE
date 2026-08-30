using System.Collections.Generic;
using UnityEngine;

public class NPCOrientationHandler : MonoBehaviour
{
    private List<Transform> NPCs;
    [SerializeField] private List<Transform> NPCsbef = new List<Transform>();
    [SerializeField] private List<Transform> NPCsaft = new List<Transform>();

    private Rigidbody rb;  // PlayerのRigidbodyコンポーネントを保存する変数

    public Playermanager playermanager; 

    private bool isOriParachute = false;
    private bool isRight = false;
    private bool isLeft = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        NPCs = NPCsbef;
    }

    void Update()
    {
        float targetZ = 0f;

        if (isRight)
        {
            targetZ = -40f; // 右移動時のZ角度
        }
        else if (isLeft)
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
                if (playermanager.GetIsParachute() && !isOriParachute)
                {
                    manager.SetRoll(0f);
                    manager.SetParForm();
                }
                else
                {
                    manager.SetRoll(targetZ); // ★ここで横の角度をセット！
                }
            }
        }

        if(playermanager.GetIsParachute() && !isOriParachute)
        {
            isOriParachute = true;
            NPCs = NPCsaft;
        }
    }

    public void SetIsRightLeft(bool right ,bool left)
    {
        isRight = right;
        isLeft = left;
    }
}
