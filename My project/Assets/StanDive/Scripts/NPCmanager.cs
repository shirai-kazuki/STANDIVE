using UnityEngine.InputSystem;
using UnityEngine;

public class NPCmanager : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 0.1f; // 移動スピード
    private float height;
    private bool isdiving = false;
    // インスペクターで対象のスクリプトを指定する
    [SerializeField] private MonoBehaviour targetScript;
    private Animator animator;
    public bool isNext = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        //初期の高さを保存
        height = transform.position.y;
    }

    void Update()
    {
        // 例：スペースキーを押したら次へ進むフラグを真にする
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            isNext = true;
        }
        if (isNext)
        {
            animator.SetBool("isNext", true);
        }

        if (isdiving)
        {
            Move();
        }
        
        if(transform.position.y < height - 10f)
        {
            // スクリプトをアクティブ（有効化）にする
            targetScript.enabled = true;
            // 自分自身を非アクティブ（消す/隠す）にする
            gameObject.SetActive(false);
        }
    }

    void Move()
    {
        // 常に正面に進む
        Vector3 direction = Vector3.forward * speed;
        transform.Translate(direction * Time.deltaTime);
    }

    void SetIsdiving()
    {
        isdiving = true;
    }
}
