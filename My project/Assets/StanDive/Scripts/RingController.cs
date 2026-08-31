using UnityEngine;

public class RingController : MonoBehaviour
{
    [SerializeField] private GameObject ringPrefab; // リングのプレハブ
    [SerializeField] private float spawnDistance = 400.0f; // プレイヤーの前方に出現させる距離
    public AudioSource audioSource;
    public AudioClip oneShotClip; // 1回用（SEなど）
    
    private GameObject currentRing;
    private int score = 0;

    // 1回だけ実行するためのフラグ
    private bool hasProcessed = false; 

    void Start()
    {
        
    }

    void Update()
    {
        if(transform.position.y < 3100f && !hasProcessed)
        {
            SpawnRing();
            hasProcessed = true;
        }

        if (currentRing == null) return;

        if (transform.position.y < currentRing.transform.position.y)
        {
            ResetRing();
        }
    }

    void SpawnRing()
    {
        if (ringPrefab != null)
        {
            // Playerの前方、ランダムな高さ（Y軸）にリングを生成
            Vector3 spawnPos = transform.position;
            spawnPos.y -= spawnDistance; 
            spawnPos.x += Random.Range(-100.0f, 100.0f);

            // Playerと同じ向きにリングを生成
            if (transform.position.y > 700f)
            {
                currentRing = Instantiate(ringPrefab, spawnPos, Quaternion.Euler(0f, 0f, 0f));
            }
        }
    }

    // リングをくぐった（トリガーに触れた）時の処理
    private void OnTriggerEnter(Collider other)
    {
        // タグが「Ring」の場合
        if (other.CompareTag("Ring"))
        {
            // 得点を加算（例：100点）
            score += 100;
            Debug.Log("Score: " + score);

            PlayOneShot();

            // くぐったリングを消去
            Destroy(other.gameObject);

            // 新しいリングを出現させる
            SpawnRing();
        }
    }

    private void ResetRing()
    {
        Destroy(currentRing);
        SpawnRing();
    }

    // 2. 1回だけ再生する（SE向け：重なり可能）
    public void PlayOneShot()
    {
        // loop設定に関係なく1回だけ再生
        audioSource.PlayOneShot(oneShotClip, 1f); // 1fは音量の倍率（0.0～1.0）
    }
}

