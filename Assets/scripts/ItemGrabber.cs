using UnityEngine;

public class ItemGrabber : MonoBehaviour
{
    public static ItemGrabber Instance;

    public float interactRange = 3f;
    public float holdDistance = 2f;
    public float followSpeed = 15f;

    private DroppedItem heldItem = null;
    private Rigidbody heldRb = null;
    private Collider heldCol = null;
    private bool placing = false;
    private Camera cam;

    // Gun.cs から参照してクリックを抑制
    public bool IsHolding => heldItem != null || placing;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        if (heldItem != null)
        {
            // 掴んでいるアイテムをカメラ前方に追従させる
            Vector3 holdPos = cam.transform.position + cam.transform.forward * holdDistance;
            heldItem.transform.position = Vector3.Lerp(
                heldItem.transform.position, holdPos, Time.deltaTime * followSpeed);

            // 左クリック：設置（同フレームの発砲を防ぐためLateUpdateで実行）
            if (Input.GetMouseButtonDown(0))
            {
                placing = true;
                return;
            }

            // Gキー再押し：その場にドロップ
            if (Input.GetKeyDown(KeyCode.G))
                Release();
        }
        else if (!placing)
        {
            // Gキー：落ちているアイテムを掴む
            if (Input.GetKeyDown(KeyCode.G))
            {
                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, interactRange))
                {
                    DroppedItem di = hit.collider.GetComponent<DroppedItem>();
                    if (di != null)
                        Grab(di);
                }
            }
        }
    }

    // LateUpdate で解放することで、同フレームの Gun.cs 発砲を抑制
    void LateUpdate()
    {
        if (placing)
        {
            Release();
            placing = false;
        }
    }

    void Grab(DroppedItem item)
    {
        heldItem = item;
        heldRb = item.GetComponent<Rigidbody>();
        heldCol = item.GetComponent<Collider>();

        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.useGravity = false;
            heldRb.velocity = Vector3.zero;
        }
        if (heldCol != null)
            heldCol.enabled = false;
    }

    void Release()
    {
        if (heldItem == null) return;

        if (heldCol != null)
            heldCol.enabled = true;

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = true;
        }

        heldItem = null;
        heldRb = null;
        heldCol = null;
    }
}
