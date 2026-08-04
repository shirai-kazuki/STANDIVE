using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FadeController : MonoBehaviour
{
    // 先ほど作ったFadePanelのImageをインスペクターから登録する
    [SerializeField] private Image fadeImage;

    // 暗転にかける時間（秒）
    [SerializeField] private float fadeDuration = 1.0f;

    public Transform playerTransform; // プレイヤーのTransform（インスペプターからアタッチ）

    void Update()
    {
        if (playerTransform == null) return;

        // 1. プレイヤーとこのオブジェクト（またはTerrainの中心など）の距離を計算
        float distance = playerTransform.position.y;

        if (distance < 1f) // 1m未満で暗転開始
        {
            StartFadeOut();
        }
    }

    // 外部から暗転を呼び出すためのメソッド
    public void StartFadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }

    // 徐々に暗くする処理
    private IEnumerator FadeOutRoutine()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        // 念のためレイキャストを有効にして、暗転中の画面クリックを防止
        fadeImage.raycastTarget = true;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // アルファ値を0から1へ徐々に変化させる
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null; // 1フレーム待つ
        }

        // 完全に真っ黒にする
        color.a = 1f;
        fadeImage.color = color;
    }
}
