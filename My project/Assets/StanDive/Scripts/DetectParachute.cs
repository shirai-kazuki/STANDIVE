using UnityEngine;

public class DetectParachute : MonoBehaviour
{
    // 【ポイント】Unityの画面（インスペクター）で右手か左手かを選べます
    public enum HandSide { Left, Right }
    [Header("このオブジェクトがどちらの手か選んでください")]
    public HandSide handSide;

    [Header("設定値（調整用）")]
    public float downSpeedThreshold = 1.5f; // 下降速度の基準
    public float downTimeout = 1.0f;        // 制限時間（この時間内に振り切る）

    public Playermanager playermanager; 

    private Vector3 lastPosition;
    private float downTimer = 0f;
    private bool nowMovingDown = false;

    void Start()
    {
        // 最初の位置を記録
        lastPosition = transform.position;
    }

    void Update()
    {
        // 今のフレームでの移動速度（y軸：上下の動き）を計算
        Vector3 playerPosition = playermanager.GetPlayerPosition();
        Vector3 currentPosition = transform.position - playerPosition;
        float speedY = (currentPosition.y - lastPosition.y) / Time.deltaTime;

        // 2. 制限時間のタイマーをすすめる
        if (nowMovingDown)
        {
            downTimer += Time.deltaTime;
            if (downTimer > downTimeout)
            {
                ResetWave(); // 時間切れならリセット
            }
        }

        // 一定以上の速さで動いているかチェック
        if (Mathf.Abs(speedY) > downSpeedThreshold)
        {
            // 動いている向き（上か下か）を判定
            nowMovingDown = speedY > 0;

            // 前のフレームと逆の向きに切り替わった瞬間を捉える（往復の検出）
            if (nowMovingDown)
            {
                // 管理スクリプトに手を下げたかを送る
                if (handSide == HandSide.Left)
                {
                    playermanager.SetisLeftHandDown(true);
                }
                else if (handSide == HandSide.Right)
                {
                    playermanager.SetisRightHandDown(true);
                }
            }
            else
            {
                // 管理スクリプトに手を下げたかを送る
                if (handSide == HandSide.Left)
                {
                    playermanager.SetisLeftHandDown(false);
                }
                else if (handSide == HandSide.Right)
                {
                    playermanager.SetisRightHandDown(false);
                }
            }
        }

        // 次のフレームのために今の位置と速度を保存
        lastPosition = currentPosition;
    }

    void ResetWave()
    {
        nowMovingDown = false;
        downTimer = 0f;
    }
}
