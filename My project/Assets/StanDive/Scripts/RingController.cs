using UnityEngine;
using System.Collections; // 【追加】3秒後に音を止めるため

public class RingController : MonoBehaviour
{
    [SerializeField] private GameObject ringPrefab; // リングのプレハブ
    [SerializeField] private float spawnDistance = 400.0f; // プレイヤーの前方に出現させる距離

    [Tooltip("最初の景色から順番にSkyboxマテリアルを登録します")]
    [SerializeField] private Material[] skyboxMaterials;

    // 【追加】各Skyboxに対応する音
    [Header("Skybox切り替え音")]
    [Tooltip("Skyboxと同じ順番で音声ファイルを登録します")]
    [SerializeField] private AudioClip[] skyboxAudioClips;

    // 【追加】Skybox切り替え音専用のAudioSource
    [SerializeField] private AudioSource skyboxAudioSource;

    // 【追加】音を再生する時間
    [SerializeField] private float skyboxAudioDuration = 3.0f;

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
        if (transform.position.y < 3100f && !hasProcessed)
        {
            SpawnRing();
            hasProcessed = true;
        }

        if (playermanager.GetIsParachute())
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
                currentRing = Instantiate(
                    ringPrefab,
                    spawnPos,
                    Quaternion.Euler(0f, 0f, 0f)
                );
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


        // リングをくぐるたびに次のSkyboxへ変更
        ChangeToNextSkybox();

        // 【追加】変更後のSkyboxに対応する音を再生
        PlaySkyboxAudio(currentSkyboxIndex);

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

    // 【追加】対応するSkyboxの音を再生
    private void PlaySkyboxAudio(int index)
    {
        if (skyboxAudioSource == null ||
            skyboxAudioClips == null ||
            index < 0 ||
            index >= skyboxAudioClips.Length ||
            skyboxAudioClips[index] == null)
        {
            return;
        }

        // 前の音が再生中なら停止
        skyboxAudioSource.Stop();

        skyboxAudioSource.clip = skyboxAudioClips[index];
        skyboxAudioSource.loop = false;
        skyboxAudioSource.Play();

        // 約3秒後に停止する
        StartCoroutine(
            StopSkyboxAudioAfterSeconds(
                skyboxAudioClips[index],
                skyboxAudioDuration
            )
        );
    }

    // 【追加】指定時間後に音を停止
    private IEnumerator StopSkyboxAudioAfterSeconds(
        AudioClip playingClip,
        float seconds
    )
    {
        yield return new WaitForSeconds(seconds);

        // その間に別のSkybox音へ変わっていない場合だけ停止
        if (skyboxAudioSource != null &&
            skyboxAudioSource.clip == playingClip)
        {
            skyboxAudioSource.Stop();
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
        audioSource.PlayOneShot(oneShotClip, 1f);
    }
}