using UnityEngine;

public class DisplayingWarnings : MonoBehaviour
{
    
    public GameObject warningPanel; // 警告パネルのGameObjectをインスペクターからアタッチ
    public Playermanager playermanager; 

    void Start()
    {
        warningPanel.SetActive(false); // 初期状態では警告パネルを非表示
    }

    void Update()
    {
        // プレイヤーの位置を取得
        Vector3 playerPosition = transform.position;

        // ここで警告を表示する条件を設定（例: y座標が特定の値以下の場合）
        if (playerPosition.y < 300f && !playermanager.isParachute) // 300未満で警告表示
        {
            warningPanel.SetActive(true); // 警告パネルを表示
        }
        else
        {
            warningPanel.SetActive(false); // 条件を満たさない場合は非表示
        }

    }
}
