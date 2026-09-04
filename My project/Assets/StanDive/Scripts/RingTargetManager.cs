using System.Collections.Generic;
using UnityEngine;

public class RingTargetManager : MonoBehaviour
{
    // [SerializeField]をつけることでインスペクターに表示されます
    [SerializeField] private List<Transform> targetChildren = new List<Transform>();

    [SerializeField] private float minHeight = -30f;
    [SerializeField] private float maxHeight = -5f;
    [SerializeField] private float intervalSeconds = 5f;

    // 現在計測中の時間を記録する変数（インスペクターで確認可能）
    [SerializeField] private float currentTimer = 0f;

    public Playermanager playermanager; 

    void Start()
    {
        RandomizeHeights();
    }

    void Update()
    {
        // 前のフレームからの経過時間を足していく
        currentTimer += Time.deltaTime;

        // パラシュートが開いたらすぐに変える
        if (playermanager.GetIsParachute())
        {
            RandomizeHeights();
            // タイマーをリセット（0に戻す）
            currentTimer = 0f;
            return;
        }

        // 計測時間が設定した秒数（5秒）を超えたかチェック
        if (currentTimer >= intervalSeconds)
        {
            // 高さをランダムに変える処理を実行
            RandomizeHeights();

            // タイマーをリセット（0に戻す）
            currentTimer = 0f;
        }
    }

    private void RandomizeHeights()
    {
        // リストが空の場合はエラーを防ぐために処理をスキップ
        if (targetChildren == null || targetChildren.Count == 0) return;

        // インスペクターで登録されたオブジェクトをループ処理
        foreach (Transform childTransform in targetChildren)
        {
            if (childTransform == null) continue; // 空のスロット対策

            Vector3 pos = childTransform.position;
            if(playermanager.GetIsParachute())
            {
                if (childTransform.name == "Target7")
                {
                    pos.y = transform.position.y + 200f;
                }
                else
                {
                    pos.y = transform.position.y;
                }
            }
            else
            {
                pos.y = transform.position.y + Random.Range(minHeight, maxHeight) - 20f;
            }
            
            childTransform.position = pos;
        }
    }

}
