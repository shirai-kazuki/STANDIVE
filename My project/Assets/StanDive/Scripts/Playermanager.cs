using UnityEngine;
using UnityEngine.InputSystem;

public class Playermanager : MonoBehaviour
{
    private Rigidbody rb;  // プレイヤーのRigidbodyコンポーネントを保存する変数
    private float maxhorizontalSpeed = 15f;// プレイヤーの最大横移動速度を保存する変数
    private float horizontalSpeed;// プレイヤーの横移動の速度を保存する変数
    private float heightDifference;// 左手と右手の高さの差を保存する変数
    private float height;// プレイヤーの初期の高さを保存する変数
    private float downheight;// パラシュート展開の高さを保存する変数
    public int progressStep = 0; // 進行状況を管理する変数
    public float leftHandHeight;// 左手と右手の高さを保存する変数
    public float rightHandHeight;
    public Transform target; // 目的地のTransformを保存する変数
    public float arrivalThreshold = 0.1f; // 到達とみなす距離
    private bool isHandWave = false;// 手を振ったかどうかを保存する変数
    [Header("傾きの設定")]
    public float tiltSpeed = 30f; // 1秒間に傾く度数
    private Quaternion tiltUp;
    private Quaternion tiltDown;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //空気抵抗を0.1にする
        rb.linearDamping = 0.1f; 
        //初期の高さを保存
        height = transform.position.y;
        downheight = height - 3200f;

        rb.maxAngularVelocity = 1000f; // ブレーキ解除

        // ゲーム開始時に、角度のデータを1度だけ作って保存しておく（エコ！）
        tiltUp = Quaternion.Euler(30f, 0f, 0f);
        tiltDown = Quaternion.Euler(0f, 0f, 0f);
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

    // 手のポーズから左右の速度を計算する処理
    private void CalculateMovement()
    {
        // 左手と右手の高さの差を計算
        heightDifference = Mathf.Abs(leftHandHeight - rightHandHeight);

        // 上下の速度を計算（高さの差が大きいほど速くなる）
        horizontalSpeed = Mathf.Clamp(heightDifference / 0.1f * maxhorizontalSpeed, 0f, maxhorizontalSpeed);
    }

    // プレイヤーを移動させる関数
    private void MovePlayer()
    {
        CalculateMovement();
        if (transform.position.y >= height)
        {
            rb.AddForce(transform.forward * 30f); // 常に正面に飛び出す
        }else
        {
            TiltPlayer(); // プレイヤーを傾ける処理を呼び出す
        }

        //右に進む
        if (leftHandHeight > rightHandHeight) // 左手が右手よりも高い場合
        {
            rb.AddForce(transform.right * horizontalSpeed);
        }
        //左に進む
        if (rightHandHeight > leftHandHeight) // 右手が左手よりも高い場合
        {
            rb.AddForce(-transform.right * horizontalSpeed);
        }
    }

    private void TiltPlayer()
    {
        // 1. 保存しておいた角度から、どちらを目指すか「選ぶ」だけで済むようにする
        Quaternion targetRotation = tiltUp;
        
        if (transform.position.y < downheight) 
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
        if(isHandWave){
            progressStep = 2; // 次の処理に進む
        }
    }

    public void ThirdProcess()
    {
        //3番目の処理
        MovePlayer();

        if (transform.position.y < downheight)
        {
            // プレイヤーが一定の高さより下に落ちたら、子オブジェクトをアクティブにする
            ActiveChildByName("Parachute");
            // プレイヤーが一定の高さより下に落ちたら、子オブジェクトを非アクティブにする
            DeactivateChildByName("WindPressure");
   
            //空気抵抗を1にする
            rb.linearDamping = 1f;
        }
    }

}
