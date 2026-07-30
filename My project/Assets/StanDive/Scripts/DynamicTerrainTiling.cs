using UnityEngine;

public class DynamicTerrainTiling : MonoBehaviour
{
    public Terrain targetTerrain;
    public Transform playerTransform; // プレイヤーのTransform（インスペプターからアタッチ）

    [Header("対象レイヤーのインデックス")]
    public int textureIndex = 0; // 最初の一枚は0番

    [Header("距離の基準（メートル）")]
    public float minDistance = 0f;    // これより近いと一番細かくなる
    public float maxDistance = 3000f;  // これより遠いと一番粗くなる

    [Header("タイリングサイズの設定")]
    public float minTileSize = 5f;    // プレイヤーが最も近い時のSize (高解像度)
    public float maxTileSize = 1000f;  // プレイヤーが最も遠い時のSize (マクロ化)

    [Header("段階的な変化のステップ幅")]
    public float sizeStep = 200f;     // ★この数値（メートル）刻みでカチッカチッと変化させます

    void Start()
    {
        if (targetTerrain == null) targetTerrain = Terrain.activeTerrain;
        
        // プレイヤーが未設定ならMainCameraを自動割り当て
        if (playerTransform == null && Camera.main != null) 
            playerTransform = Camera.main.transform;
    }

    void Update()
    {
        if (playerTransform == null || targetTerrain == null) return;

        // 1. プレイヤーとこのオブジェクト（またはTerrainの中心など）の距離を計算
        float distance = playerTransform.position.y;

        // 2. 距離の割合（0.0 ～ 1.0）を算出
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);

        // 3. 割合に応じてタイリングサイズを線形補間（Lerp）
        float currentSize = Mathf.Lerp(minTileSize, maxTileSize, t);

        // ★【修正ポイント1】計算されたサイズを「指定したステップ幅」に四捨五入して丸めます
        // 例：sizeStepが200の場合、サイズは 5, 200, 400, 600, 800, 1000 のように段階的な固定値になります
        currentSize = Mathf.Round(currentSize / sizeStep) * sizeStep;
        
        // 最小値と最大値の範囲から超えないように制限（保護コード）
        currentSize = Mathf.Clamp(currentSize, minTileSize, maxTileSize);

        // 4. TerrainLayerに適用
        ApplyTilingSize(new Vector2(currentSize, currentSize));
    }

    void ApplyTilingSize(Vector2 newSize)
    {
        TerrainData terrainData = targetTerrain.terrainData;
        TerrainLayer[] layers = terrainData.terrainLayers;

        if (textureIndex >= 0 && textureIndex < layers.Length)
        {
            // ★【修正ポイント2】条件式を「少しでもサイズが変わったら（> 0.1f）」に戻します
            // 上のUpdate側で数値を「200刻み」などの段階的な値に固定したため、
            // ここは敏感に戻しておくことで、プレイヤーが止まった位置の目標サイズへ最後まできれいに適用されます。
            if (Vector2.Distance(layers[textureIndex].tileSize, newSize) > 0.1f)
            {
                layers[textureIndex].tileSize = newSize;
                terrainData.terrainLayers = layers; // 再代入して確定
                
                // 画面を即座に更新させる命令（これがないと見た目が変わらない場合があります）
                targetTerrain.Flush();
            }
        }
    }
}
