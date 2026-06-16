using UnityEngine;

public class GetHandHeight : MonoBehaviour
{
    private OVRHand ovrHand;
    // 【ポイント】Unityの画面（インスペクター）で右手か左手かを選べます
    public enum HandSide { Left, Right }
    [Header("このオブジェクトがどちらの手か選んでください")]
    public HandSide handSide;

    public Playermanager playermanager; 

    void Start()
    {
        // OVRHand コンポーネントを取得
        ovrHand = GetComponent<OVRHand>();

    }

    void Update()
    {
        // 手がトラッキングされているか確認
        if (ovrHand != null && ovrHand.IsTracked && playermanager != null)
        {
            // 手のひら（Hand Root）のY座標を取得する
            float handHeight = transform.position.y;

            // 管理スクリプトに「高さ」を送る
            if (handSide == HandSide.Left)
            {
                playermanager.SetLeftHeight(handHeight);
            }
            else if (handSide == HandSide.Right)
            {
                playermanager.SetRightHeight(handHeight);
            }
        }
    }
}
