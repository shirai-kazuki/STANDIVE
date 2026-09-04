using UnityEngine;

public class RingController : MonoBehaviour
{
    [SerializeField] private GameObject ringPrefab; // リングのプレハブ
    [SerializeField] private float spawnDistance = 400.0f; // プレイヤーの前方に出現させる距離

    [Tooltip("最初の景色から順番にSkyboxマテリアルを登録します")]
    [SerializeField] private Material[] skyboxMaterials;

    public AudioSource audioSource;
    public AudioClip oneShotClip; // 1回用（SEなど）
    
    private GameObject currentRing;
    private int score = 0;

    // 現在表示しているSkyboxの番号
    private int currentSkyboxIndex = 0;

    // 1回だけ実行するためのフラグ
    private bool hasProcessed = false; 

    public Playermanager playermanager; 

    void Start()
    {
        // ゲーム開始時は最初の景色を表示
        if (skyboxMaterials != null && skyboxMaterials.Length > 0)
        {
            ChangeSkybox(0);
        }
    }

    void Update()
    {
        if(transform.position.y < 3100f && !hasProcessed)
        {
            SpawnRing();
            hasProcessed = true;
        }

        if(playermanager.GetIsParachute())
        {
            Destroy(currentRing);
            currentRing = null;
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
        if (!other.CompareTag("Ring"))
        {
            return;
        }

        PlayOneShot();

        // リングをくぐるたびに次のSkyboxへ変更
        ChangeToNextSkybox();

        // くぐったリングを削除
        Destroy(other.gameObject);
        currentRing = null;

        // 次のリングを生成
        SpawnRing();
    }

    private void ChangeToNextSkybox()
    {
        if (skyboxMaterials == null || skyboxMaterials.Length == 0)
        {
            return;
        }

        currentSkyboxIndex++;

        // 最後のSkyboxの次は、最初の景色に戻す
        if (currentSkyboxIndex >= skyboxMaterials.Length)
        {
            currentSkyboxIndex = 0;
        }

        ChangeSkybox(currentSkyboxIndex);
    }

    public void ChangePSkybox(int index)
    {
        ChangeSkybox(index);
    }

    private void ChangeSkybox(int index)
    {
        if (skyboxMaterials == null ||
            index < 0 ||
            index >= skyboxMaterials.Length ||
            skyboxMaterials[index] == null)
        {
            return;
        }

        RenderSettings.skybox = skyboxMaterials[index];

        // Skyboxの表示をすぐに更新
        DynamicGI.UpdateEnvironment();
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

