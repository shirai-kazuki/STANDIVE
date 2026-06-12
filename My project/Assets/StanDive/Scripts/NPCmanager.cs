using UnityEngine;

public class NPCmanager : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 0.1f; // 移動スピード
    private float height;
    // インスペクターで対象のスクリプトを指定する
    [SerializeField] private MonoBehaviour targetScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //初期の高さを保存
        height = transform.position.y;
    }

    void Update()
    {
        // 常に正面に進む
        rb.AddForce(transform.forward * speed);

        if (transform.position.y < height - 50f)
        {
            // スクリプトをアクティブ（有効化）にする
            targetScript.enabled = true;
            // 自分自身を非アクティブ（消す/隠す）にする
            gameObject.SetActive(false);
        }
    }
}
