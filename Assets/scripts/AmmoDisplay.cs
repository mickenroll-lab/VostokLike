using UnityEngine;
using TMPro;

// 武器グリッドセルに残弾数を表示するコンポーネント。
// セルにAddComponentして Show()/Hide() を呼ぶだけで動作する。
//
// === 将来のマガジンシステム拡張ポイント ===
// 現在：currentAmmo / magazineSize の2値で表示
// 将来：Magazine ScriptableObject を導入する場合、
//   Show(Magazine mag) overload を追加し、
//   mag.currentAmmo / mag.capacity で表示を切り替える。
//   InventoryItem.storedAmmo → InventoryItem.magazine に移行する想定。
// ==========================================
public class AmmoDisplay : MonoBehaviour
{
    private TextMeshProUGUI ammoText;

    void Awake()
    {
        GameObject textObj = new GameObject("AmmoText");
        textObj.transform.SetParent(transform, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(3f, 3f);
        rt.offsetMax = new Vector2(-3f, -3f);

        ammoText = textObj.AddComponent<TextMeshProUGUI>();
        ammoText.fontSize = 26;
        ammoText.color = Color.yellow;
        ammoText.alignment = TextAlignmentOptions.BottomLeft;
        ammoText.raycastTarget = false;
        ammoText.gameObject.SetActive(false);
    }

    public void Show(int current, int max)
    {
        if (ammoText == null) return;
        ammoText.gameObject.SetActive(true);
        ammoText.text = current + "/" + max;
        ammoText.color = current == 0 ? Color.red : Color.yellow;
    }

    public void Hide()
    {
        if (ammoText == null) return;
        ammoText.gameObject.SetActive(false);
    }
}
