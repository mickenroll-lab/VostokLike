using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string itemName; // itemId‚ðŠi”[
    public bool fromInventory;
    public Inventory inventory;
    public BoxContainer boxContainer;
    public GameObject dragGhost;

    private CanvasGroup canvasGroup;
    private Image ghostImage;
    private TextMeshProUGUI ghostText;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragGhost == null) return;

        ghostImage = dragGhost.GetComponent<Image>();
        ghostText = dragGhost.GetComponentInChildren<TextMeshProUGUI>();

        Image myImage = GetComponent<Image>();
        if (ghostImage != null && myImage != null)
            ghostImage.color = myImage.color;

        TextMeshProUGUI myText = GetComponentInChildren<TextMeshProUGUI>();
        if (ghostText != null && myText != null)
            ghostText.text = myText.text;

        dragGhost.SetActive(true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
            dragGhost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
            dragGhost.SetActive(false);
        canvasGroup.blocksRaycasts = true;
        Debug.Log("OnEndDragŒÄ‚Î‚ê‚½");
    }
}