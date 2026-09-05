using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class RingController : MonoBehaviour
{
    [Header("リング設定")]
    [SerializeField]
    private GameObject ringPrefab;

    [SerializeField]
    private float spawnDistance = 578.0f;

    [Header("Skybox設定")]
    [Tooltip("最初の景色から順番にSkyboxマテリアルを登録します")]
    [SerializeField]
    private Material[] skyboxMaterials;

    [Header("リング通過時の音")]
    [Tooltip("リングを通過したときに再生する音です")]
    [SerializeField]
    private AudioClip ringPassClip;

    [Tooltip("リング通過音を再生するAudioSourceです")]
    [SerializeField]
    private AudioSource ringAudioSource;

    [Tooltip("リング通過音を再生する秒数です")]
    [SerializeField]
    private float ringAudioDuration = 3.0f;

    [Header("Player設定")]
    [SerializeField]
    private Playermanager playermanager;


    [Header("リング通過時の加速ぼかし")]
    [SerializeField] private Volume accelerationBlurVolume;

    [SerializeField] private float blurFadeInTime = 0.15f;
    [SerializeField] private float blurHoldTime = 0.2f;
    [SerializeField] private float blurFadeOutTime = 0.65f;

    private Coroutine blurCoroutine;

    private GameObject currentRing;

    // 現在表示しているSkyboxの番号
    private int currentSkyboxIndex = 0;

    // 最初のリングを1回だけ生成するためのフラグ
    private bool hasProcessed = false;

    // 音声停止用コルーチン
    private Coroutine stopAudioCoroutine;

    void Start()
    {
        // ゲーム開始時は最初のSkyboxを表示
        // 開始時には音を鳴らさない
        if (skyboxMaterials != null &&
            skyboxMaterials.Length > 0)
        {
            currentSkyboxIndex = 0;
            ChangeSkybox(currentSkyboxIndex);
        }
        if (accelerationBlurVolume != null)
        {
            accelerationBlurVolume.weight = 0f;
        }
    }

    void Update()
    {
        // 高度3100未満になったときに最初のリングを生成
        if (transform.position.y < 3100f && !hasProcessed)
        {
            SpawnRing();
            hasProcessed = true;
        }

        // パラシュートを開いたらリングを削除
        if (playermanager != null &&
            playermanager.GetIsParachute())
        {
            if (currentRing != null)
            {
                Destroy(currentRing);
                currentRing = null;
            }
        }

        if (currentRing == null)
        {
            return;
        }

        // リングをくぐらずに下へ通過した場合
        if (transform.position.y <
            currentRing.transform.position.y)
        {
            ResetRing();
        }
    }

    private void SpawnRing()
    {
        if (ringPrefab == null)
        {
            return;
        }

        if (playermanager == null)
        {
            Debug.LogWarning(
                "RingControllerにPlayermanagerが登録されていません。"
            );
            return;
        }

        // プレイヤーより下方にリングを配置
        Vector3 spawnPos = transform.position;
        spawnPos.y -= spawnDistance;

        // リングの左右位置をランダムにする
        spawnPos.x += Random.Range(-100.0f, 100.0f);

        // パラシュート展開高度より十分上にいる場合のみ生成
        if (transform.position.y >
            playermanager.GetDownheight() + spawnDistance)
        {
            currentRing = Instantiate(
                ringPrefab,
                spawnPos,
                Quaternion.Euler(0f, 0f, 0f)
            );
        }
    }

    // リングをくぐったときの処理
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ring"))
        {
            return;
        }

        // 次のSkyboxへ変更
        ChangeToNextSkybox();

        // リング通過時のみ共通の音を再生
        PlayRingPassAudio();

        PlayAccelerationBlur();

        // くぐったリングを削除
        Destroy(other.gameObject);
        currentRing = null;

        // 次のリングを生成
        SpawnRing();


    }

    private void ChangeToNextSkybox()
    {
        if (skyboxMaterials == null ||
            skyboxMaterials.Length == 0)
        {
            return;
        }

        currentSkyboxIndex++;

        // 最後のSkyboxの次は最初に戻る
        if (currentSkyboxIndex >= skyboxMaterials.Length)
        {
            currentSkyboxIndex = 0;
        }

        ChangeSkybox(currentSkyboxIndex);
    }

    // 外部から呼び出すランダムSkybox変更
    // この処理では音を鳴らさない
    public void ChangePSkybox()
    {
        if (skyboxMaterials == null ||
            skyboxMaterials.Length == 0)
        {
            return;
        }

        // Skyboxが1個しかない場合
        if (skyboxMaterials.Length == 1)
        {
            currentSkyboxIndex = 0;
            ChangeSkybox(currentSkyboxIndex);
            return;
        }

        int nextIndex;

        do
        {
            nextIndex = Random.Range(
                0,
                skyboxMaterials.Length
            );
        }
        while (nextIndex == currentSkyboxIndex);

        currentSkyboxIndex = nextIndex;
        ChangeSkybox(currentSkyboxIndex);
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

    // リング通過時の音を再生
    private void PlayRingPassAudio()
    {
        if (ringAudioSource == null)
        {
            Debug.LogWarning(
                "Ring Audio Sourceが登録されていません。"
            );
            return;
        }

        if (ringPassClip == null)
        {
            Debug.LogWarning(
                "Ring Pass Clipが登録されていません。"
            );
            return;
        }

        // 前回の停止処理が残っていれば解除
        if (stopAudioCoroutine != null)
        {
            StopCoroutine(stopAudioCoroutine);
            stopAudioCoroutine = null;
        }

        // 前の音を停止して最初から再生
        ringAudioSource.Stop();
        ringAudioSource.clip = ringPassClip;
        ringAudioSource.loop = false;
        ringAudioSource.Play();

        // 指定秒数後に停止
        stopAudioCoroutine = StartCoroutine(
            StopRingAudioAfterSeconds(ringAudioDuration)
        );
    }

    private IEnumerator StopRingAudioAfterSeconds(
        float seconds
    )
    {
        yield return new WaitForSeconds(seconds);

        if (ringAudioSource != null)
        {
            ringAudioSource.Stop();
        }

        stopAudioCoroutine = null;
    }

    private void ResetRing()
    {
        if (currentRing != null)
        {
            Destroy(currentRing);
            currentRing = null;
        }

        // 失敗時はSkyboxを変更せず、次のリングを生成
        SpawnRing();
    }

    private void PlayAccelerationBlur()
    {
        if (accelerationBlurVolume == null)
        {
            Debug.LogWarning(
                "Acceleration Blur Volumeが登録されていません。"
            );
            return;
        }

        if (blurCoroutine != null)
        {
            StopCoroutine(blurCoroutine);
        }

        blurCoroutine = StartCoroutine(AccelerationBlurRoutine());
    }

    private IEnumerator AccelerationBlurRoutine()
    {
        // ぼかしを徐々に強くする
        float elapsedTime = 0f;

        while (elapsedTime < blurFadeInTime)
        {
            elapsedTime += Time.deltaTime;

            accelerationBlurVolume.weight = Mathf.Lerp(
                0f,
                1f,
                elapsedTime / blurFadeInTime
            );

            yield return null;
        }

        accelerationBlurVolume.weight = 1f;

        // 最大のぼかしを少し維持
        yield return new WaitForSeconds(blurHoldTime);

        // ぼかしを徐々に弱くする
        elapsedTime = 0f;

        while (elapsedTime < blurFadeOutTime)
        {
            elapsedTime += Time.deltaTime;

            accelerationBlurVolume.weight = Mathf.Lerp(
                1f,
                0f,
                elapsedTime / blurFadeOutTime
            );

            yield return null;
        }

        accelerationBlurVolume.weight = 0f;
        blurCoroutine = null;
    }
}