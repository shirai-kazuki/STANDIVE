using UnityEngine;
using UnityEngine.InputSystem;

public class Playermanager : MonoBehaviour
{
    private Rigidbody rb;  // プレイヤーのRigidbodyコンポーネントを保存する変数
    private float maxhorizontalSpeed = 15f;// プレイヤーの最大横移動加速度を保存する変数
    private float maxSpeed = 50f;// プレイヤーの最大横移動速度を保存する変数
    private float fallSpeed = 1f;// プレイヤーの落下速度を保存する変数
    private float horizontalSpeed;// プレイヤーの横移動の速度を保存する変数
    private float heightDifference;// 左手と右手の高さの差を保存する変数
    private float height;// プレイヤーの初期の高さを保存する変数
    private float downheight = 0f;// パラシュート展開の高さを保存する変数
    public int progressStep = 0; // 進行状況を管理する変数
    public float leftHandHeight;// 左手と右手の高さを保存する変数
    public float rightHandHeight;
    public Transform target; // 目的地のTransformを保存する変数
    public float arrivalThreshold = 0.1f; // 到達とみなす距離
    public bool isHandWave = false;// 手を振ったかどうかを保存する変数
    private bool isLeftHandDown = false;// 左手が下に動いたかどうかを保存する変数
    private bool isRightHandDown = false;// 右手が下に動いたかどうかを保存する変数
    public bool isParachute = false;// パラシュートが開いたかどうかを保存する変数
    [Header("傾きの設定")]
    public float tiltSpeed = 60f; // 1秒間に傾く度数
    private Quaternion tiltUp;
    private Quaternion tiltDown;
    private Quaternion targetRotation;
    public AudioSource audioSource;
    public AudioClip loopClip; // ループ用（BGMなど）
    public AudioClip oneShotClip; // 1回用（SEなど）
    private float currentTimer = 0f; //時間を計測するためのタイマー変数
    private float staytTime = 3f; //待ち時間の変数

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 新Unity(2022以降)での空気抵抗の設定
        rb.linearDamping = 0.1f;

        // 初期の高さを保存
        height = transform.position.y;
        downheight = height - 2900f;

        // 安全な回転速度の制限（Unityのデフォルト、または少し早めの 20 前後が安全です）
        rb.maxAngularVelocity = 20f;

        // スピード系変数がインスペクターで0のままだった場合の安全装置（初期値を代入）
        if (horizontalSpeed == 0) horizontalSpeed = 10f;

        // ゲーム開始時に角度のデータを作っておく
        tiltUp = Quaternion.Euler(35f, 0f, 0f);
        tiltDown = Quaternion.Euler(0f, 0f, 0f);

        // 最初の目指す角度を安全に設定
        targetRotation = tiltUp;
    }

    private void FixedUpdate()
    {
        // 今の番号に合わせて、実行する処理を毎フレーム切り替える
        if (progressStep == 0)
        {
            FirstProcess();
        }
        else if (progressStep == 1)
        {
            SecondProcess();
        }
        else if (progressStep == 2)
        {
            ThirdProcess();
        }
    }

    //名前を指定して子オブジェクトをアクティブにする関数
    private void ActiveChildByName(string targetName)
    {
        Transform child = transform.Find(targetName);
        if (!child.gameObject.activeSelf)
        {
            child.gameObject.SetActive(true);
        }
    }

    //名前を指定して子オブジェクトを非アクティブにする関数
    private void DeactivateChildByName(string targetName)
    {
        Transform child = transform.Find(targetName);
        if (child.gameObject.activeSelf)
        {
            child.gameObject.SetActive(false);
        }
    }

    // 手のポーズから左右の速度を計算する処理（VR専用・安全版）
    private void CalculateMovement()
    {
        // 1. 左手と右手の高さの差を計算
        heightDifference = Mathf.Abs(leftHandHeight - rightHandHeight);

        // 2. 【安全装置】もし差がほぼ0（VR未接続や直立不動）なら、速度を0にして計算を終了する
        // これにより、この後の割り算で「0によるエラー」が起きるのを完全に防ぎます
        if (heightDifference < 0.001f)
        {
            horizontalSpeed = 0f;
            return;
        }

        // 3. 上下の速度を計算（高さの差が大きいほど速くなる）
        horizontalSpeed = Mathf.Clamp(heightDifference / 0.1f * maxhorizontalSpeed, 0f, maxhorizontalSpeed);
    }

    // プレイヤーを移動させる関数（VRゴーグル専用・安全版）
    private void MovePlayer()
    {
        if(currentTimer < staytTime)
        {
            currentTimer += Time.fixedDeltaTime;
        }

        // 1. 現在の手の高さから左右の移動速度を計算
        CalculateMovement();

        Vector3 forceDirection = Vector3.zero;

        // 右に進む（左手が右手よりも高い場合）
        if (leftHandHeight > rightHandHeight && currentTimer >= staytTime)
        {
            forceDirection += transform.right;
        }
        // 左に進む（右手が左手よりも高い場合）
        if (rightHandHeight > leftHandHeight && currentTimer >= staytTime)
        {
            forceDirection -= transform.right;
        }

        // 手の高さに差があり、かつ計算された速度が正常な時だけ安全に力を加える
        if (forceDirection != Vector3.zero && horizontalSpeed > 0f && transform.position.y > 100f)
        {
            rb.AddForce(forceDirection * horizontalSpeed);
        }


        // 2. 高さに応じた前進（Z軸）と落下（Y軸）の速度計算
        float targetVelocityZ = rb.linearVelocity.z; // 現在のZ速度をキープ

        if (transform.position.y >= height)
        {
            targetVelocityZ = 30f; // 常に正面に飛び出す
        }
        else
        {
            TiltPlayer(); // プレイヤーを傾ける処理を呼び出す
        }

        if (transform.position.y < height - 10f)
        {
            targetVelocityZ = 0f; // 前に移動しないようにする
        }

        // 3. 最後にすべての速度（X, Y, Z）を1回だけまとめて適用する
        // 【最重要】ゴーグル未接続時に rb.linearVelocity.x が「計算不能(NaN)」になるバグを防ぐため、
        // 値が正常（float型の範囲内）であるかチェックする安全装置を挟みます。
        float targetVelocityX = rb.linearVelocity.x;
        if (float.IsNaN(targetVelocityX) || float.IsInfinity(targetVelocityX))
        {
            targetVelocityX = 0f; // 壊れたデータが入っていたら安全に 0 に戻す
        }

        if (transform.position.y < 50f)
        {
            targetVelocityX = 0f; // 横に移動しないようにする
        }
        
        if (targetVelocityX > maxSpeed)
        {
            targetVelocityX = maxSpeed; // 横移動の速度がmaxSpeedを超えないように制限
        }

        if (targetVelocityX < -maxSpeed)
        {
            targetVelocityX = -maxSpeed; // 横移動の速度が-maxSpeedを下回らないように制限
        }

        // 綺麗になった速度を代入（これで絶対にフリーズしません）
        rb.linearVelocity = new Vector3(targetVelocityX, -fallSpeed, targetVelocityZ);
    }


    private void TiltPlayer()
    {
        if (isParachute)
        {
            targetRotation = tiltDown;
        }

        // 2. RotateTowardsに戻すことで、最初から最後まで「等速」で動かします
        // FixedUpdateの中なので、Time.fixedDeltaTimeを掛け算するのが一番正確です
        Quaternion nextRotation = Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            tiltSpeed * Time.fixedDeltaTime
        );

        // 3. 回転を反映
        rb.MoveRotation(nextRotation);
    }

    // 1. ループ再生する（BGM向け）
    public void PlayLoop()
    {
        audioSource.clip = loopClip;
        audioSource.loop = true; // ループを有効にする
        audioSource.Play();
    }

    // 2. 1回だけ再生する（SE向け：重なり可能）
    public void PlayOneShot()
    {
        // loop設定に関係なく1回だけ再生
        audioSource.PlayOneShot(oneShotClip, 1f); // 1fは音量の倍率（0.0～1.0）
    }

    // 3. ループを止める
    public void StopLoop()
    {
        audioSource.Stop();
    }

    // 左手用の高さをセットするための関数
    public void SetLeftHeight(float height)
    {
        leftHandHeight = height;
    }

    // 右手用の高さをセットするための関数
    public void SetRightHeight(float height)
    {
        rightHandHeight = height;
    }

    // 手を振ったかどうかをセットするための関数
    public void SetisHandWave(bool isWave)
    {
        isHandWave = isWave;
    }

    // 左手を下げたかどうかをセットするための関数
    public void SetisLeftHandDown(bool isDown)
    {
        isLeftHandDown = isDown;
    }

    // 右手を下げたかどうかをセットするための関数
    public void SetisRightHandDown(bool isDown)
    {
        isRightHandDown = isDown;
    }

    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    public bool GetIsParachute()
    {
        return isParachute;
    }

    public float GetDownheight()
    {
        return downheight;
    }

    public void FirstProcess()
    {
        //最初の処理
        // 指定した位置へ徐々に移動する
        transform.position = Vector3.MoveTowards(transform.position, target.position, 1f * Time.deltaTime);

        // 現在地と目的地の距離を計算
        float distance = Vector3.Distance(transform.position, target.position);

        // 距離が閾値以下になったら到着とする
        if (distance <= arrivalThreshold)
        {
            SetisHandWave(false); // 手を振っていない状態にリセット
            progressStep = 1; // 次の処理に進む
        }
    }

    public void SecondProcess()
    {
        //2番目の処理
        if (isHandWave)
        {
            PlayLoop(); // ループ再生を開始
            progressStep = 2; // 次の処理に進む
        }
    }

    public void ThirdProcess()
    {
        //3番目の処理
        MovePlayer();

        if (transform.position.y > downheight)
        {
            isLeftHandDown = false;// 左手が下に動いたかどうかを保存する変数
            isRightHandDown = false;// 右手が下に動いたかどうかを保存する変数
        }

        // 左手と右手が両方とも下に動いたかを確認
        if (transform.position.y < downheight && isLeftHandDown && isRightHandDown || transform.position.y < 200f)
        {
            // パラシュートが開いた状態であることにする
            isParachute = true;
            maxSpeed = 10f;
        }

        if (isParachute)
        {
            // ここでループ音の音量を小さくする（0.0 〜 1.0 の間で指定。例は 0.2）
            audioSource.volume = 0.2f;
            // 子オブジェクトをアクティブにする
            ActiveChildByName("Parachute");
            ActiveChildByName("Rope");
            // 子オブジェクトを非アクティブにする
            DeactivateChildByName("WindPressure");

            //速度を落とす
            fallSpeed = 10f;
        }
        else
        {
            fallSpeed = 30f;
        }

        if (transform.position.y < 1f)
        {
            // 着地したする
            StopLoop(); // ループ再生を停止
            PlayOneShot(); // 1回再生を開始
            progressStep = 3; // 次の処理に進む
        }
    }

}