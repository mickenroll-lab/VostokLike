using UnityEngine;
using UnityEditor;

public class CreateMagazineAsset
{
    [MenuItem("VostokLike/Create PMPMagazine Prefab")]
    static void CreatePMPMagazine()
    {
        string path = "Assets/Resources/PMPMagazine.prefab";

        GameObject go = new GameObject("PMPMagazine");
        ItemData data = go.AddComponent<ItemData>();
        data.category = ItemData.ItemCategory.Magazine;
        data.gridWidth = 1;
        data.gridHeight = 2;
        data.maxAmmo = 8;
        data.compatibleWeapon = "PMP";
        data.compatibleBullet = "9x18mm";
        data.value = 0;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        if (prefab != null)
            Debug.Log("[CreateMagazineAsset] PMPMagazine prefab作成: " + path);
        else
            Debug.LogError("[CreateMagazineAsset] prefab作成失敗");
    }
}
