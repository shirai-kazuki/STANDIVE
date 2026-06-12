using UnityEngine;

public class HandWaveDetector : MonoBehaviour
{
    [Header("設定値（調整用）")]
    public float waveSpeedThreshold = 1.5f; // 手を振る速さの基準
    public int requiredWaveCount = 3;       // 何回往復したら「手を振った」とするか
    public float waveTimeout = 1.0f;        // 制限時間（この時間内に振り切る）

    public Playermanager playermanager; 

    private Vector3 lastPosition;
    private int currentWaveCount = 0;
    private float waveTimer = 0f;
    private bool isMovingRight = false;

    void Start()
    {
        // 最初の位置を記録
        lastPosition = transform.position;
    }

    void Update()
    {
        // 1. 今のフレームでの移動速度（X軸：左右の動き）を計算
        Vector3 currentPosition = transform.position;
        float speedX = (currentPosition.x - lastPosition.x) / Time.deltaTime;
        
        // 2. 制限時間のタイマーをすすめる
        if (currentWaveCount > 0)
        {
            waveTimer += Time.deltaTime;
            if (waveTimer > waveTimeout)
            {
                ResetWave(); // 時間切れならリセット
            }
        }

        // 3. 一定以上の速さで動いているかチェック
        if (Mathf.Abs(speedX) > waveSpeedThreshold)
        {
            // 動いている向き（右か左か）を判定
            bool nowMovingRight = speedX > 0;

            // 前のフレームと逆の向きに切り替わった瞬間を捉える（往復の検出）
            if (nowMovingRight != isMovingRight)
            {
                currentWaveCount++;
                waveTimer = 0f; // 切り替わったらタイマーを巻き戻す
                isMovingRight = nowMovingRight;

                // 4. 規定の回数以上、往復したら「手を振った！」
                if (currentWaveCount >= requiredWaveCount)
                {
                    playermanager.SetisHandWave(true);
                    ResetWave();
                }
            }
        }

        // 次のフレームのために今の位置と速度を保存
        lastPosition = currentPosition;
    }

    // カウントを初期化する
    void ResetWave()
    {
        currentWaveCount = 0;
        waveTimer = 0f;
    }
}
