using UnityEngine;
using UnityEngine.InputSystem;

public class RingNPCmanager : MonoBehaviour
{
    public Transform target; 
    private float speed = 50f;
    private float arrivalThreshold = 0f; 
    private Quaternion startRotation;
    
    private float rotationSpeed = 1f; 
    private Vector3 targetPosition;

    public Playermanager playermanager;
    [SerializeField] private MonoBehaviour targetScript;

    // -------------------------------------------------------------
    // ★新しく追加：前後の傾き（X）と左右の傾き（Z）を個別に保存する変数
    // -------------------------------------------------------------
    private float targetPitchX = 0f; // 前後の傾き
    private float targetRollZ = 0f;  // 左右の傾き
    private float baseAngleY = 0f;   // 初期状態のY軸（向き）をキープ用

    void Start()
    {
        startRotation = Quaternion.Euler(0f, 0f, 0f);
        
        // 初期状態の各軸の角度を記憶しておく
        targetPitchX = startRotation.eulerAngles.x;
        baseAngleY = startRotation.eulerAngles.y;
        targetRollZ = startRotation.eulerAngles.z;
    }

    // -------------------------------------------------------------
    // ★外部や内部から「前後の傾き」をセットする関数
    // -------------------------------------------------------------
    public void SetPitch(float xAngle)
    {
        targetPitchX = xAngle;
    }

    // -------------------------------------------------------------
    // ★外部や内部から「左右の傾き」をセットする関数
    // -------------------------------------------------------------
    public void SetRoll(float zAngle)
    {
        targetRollZ = zAngle;
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;

            // パラシュートが開いているときは即座に移動する
            if (playermanager.GetIsParachute())
            {
                transform.position = target.position;
            }
            else if(target.position.y < 3400f)
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            }

            // 到着判定による「前後の傾き（X軸）」の切り替え
            float distance = transform.position.y - target.position.y;

            if (distance <= arrivalThreshold)
            {
                // 到着：初期のX角度に戻す（関数を使ってセット）
                SetPitch(startRotation.eulerAngles.x);
                speed = 2f;
            }
            else
            {
                // 移動中：初期のX角度から90度倒す（関数を使ってセット）
                SetPitch(startRotation.eulerAngles.x + 90f);
            }

            // -------------------------------------------------------------
            // ★セットされた変数（targetPitchX, targetRollZ）から最終的な角度を求める
            // -------------------------------------------------------------
            // Y軸（向き）は初期の向き、またはアニメーション等に合わせるなら現在の向き(transform.eulerAngles.y)にします
            Vector3 combinedEuler = new Vector3(targetPitchX, baseAngleY, targetRollZ);
            Quaternion finalTargetRotation = Quaternion.Euler(combinedEuler);

            // 求めた最終的な目標角度へ、スムーズに回転させる
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                finalTargetRotation, 
                Time.deltaTime * rotationSpeed
            );
        }
    }

    private void ActiveChildByName(string targetName)
    {
        Transform child = transform.Find(targetName);
        if (!child.gameObject.activeSelf)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void SetParForm()
    {
        // 親オブジェクトと同じ位置（親から見て 0, 0, 0）にリセットする
        transform.localPosition = Vector3.zero;
        // 初期のX角度から90度起こす（関数を使ってセット）
        transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        ActiveChildByName("Parachute");
        // 名前で指定してリセット
        Transform childByName = transform.Find("NPC/Skeleton/Hips/Spine/Chest/UpperChest/Neck/Head");
        if (childByName != null)
        {
            childByName.localRotation = Quaternion.identity;
        }
        // スクリプトをアクティブ（有効化）にする
        targetScript.enabled = true;
        // 自分自身のスクリプト（コンポーネント）を無効にする
        this.enabled = false;
    }
}
