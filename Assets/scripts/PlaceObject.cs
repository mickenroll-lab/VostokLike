using UnityEngine;

public class PlaceObject : MonoBehaviour
{
    public GameObject[] placeableObjects;

    private GameObject previewObject;
    private int currentIndex = 0;

    private int rotationStep = 0;
    private const int stepAngle = 15;

    private bool snapEnabled = true;
    private float heightOffset = 0f;

    private float snapBreakDistance = 3.0f;
    private float worldGridSize = 1f;

    private SnapPoint lockedMySnap = null;
    private SnapPoint lockedTargetSnap = null;

    void Start()
    {
        SpawnPreview();
    }

    void Update()
    {
        // ======================
        // ★ 追加：ハンマー制限
        // ======================
        PlayerState state = GetComponent<PlayerState>();
        if (state == null)
        {
            Debug.LogError("見つからない：" + gameObject.name);
            return;
        }
        if (state.currentItem != "Hammer")
        {
            // プレビュー消す（表示されないように）
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }

            return;
        }
        else
        {
            // ハンマーに戻したら表示復活
            if (previewObject != null)
            {
                previewObject.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeObject(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeObject(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeObject(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeObject(3);

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.Z))
        {
            heightOffset += scroll * 2f;
        }
        else
        {
            if (scroll > 0.01f) rotationStep++;
            if (scroll < -0.01f) rotationStep--;

            int max = 360 / stepAngle;
            rotationStep = (rotationStep + max) % max;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            snapEnabled = !snapEnabled;
            ClearSnapLock();
        }

        float rotationY = rotationStep * stepAngle;
        previewObject.transform.rotation = Quaternion.Euler(0, rotationY, 0);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return;

        Vector3 mousePos = hit.point;
        bool foundSnap = false;

        // ======================
        // Snap Lock
        // ======================
        if (snapEnabled && lockedMySnap != null && lockedTargetSnap != null)
        {
            Vector3 offset = previewObject.transform.position - lockedMySnap.transform.position;
            Vector3 snapPos = lockedTargetSnap.transform.position + offset;

            if (Vector3.Distance(mousePos, snapPos) < snapBreakDistance)
            {
                previewObject.transform.position = snapPos;
                foundSnap = true;
            }
            else
            {
                ClearSnapLock();
            }
        }

        // ======================
        // New Snap Search
        // ======================
        if (snapEnabled && !foundSnap)
        {
            var result = FindBestSnap();

            if (result.found)
            {
                Vector3 offset = previewObject.transform.position - result.my.transform.position;
                Vector3 snapPos = result.target.transform.position + offset;

                if (Vector3.Distance(mousePos, snapPos) < snapBreakDistance)
                {
                    previewObject.transform.position = snapPos;
                    foundSnap = true;

                    lockedMySnap = result.my;
                    lockedTargetSnap = result.target;
                }
            }
        }

        // ======================
        // Free Placement (ground + object top)
        // ======================
        if (!foundSnap)
        {
            Vector3 pos = mousePos;

            if (snapEnabled)
            {
                pos.x = Mathf.Round(pos.x / worldGridSize) * worldGridSize;
                pos.z = Mathf.Round(pos.z / worldGridSize) * worldGridSize;
            }

            Ray downRay = new Ray(pos + Vector3.up * 2f, Vector3.down);

            if (Physics.Raycast(downRay, out RaycastHit downHit, 5f))
            {
                float bottomOffset = GetBottomOffset(previewObject);
                pos.y = downHit.collider.bounds.max.y - bottomOffset;
            }
            else
            {
                float bottomOffset = GetBottomOffset(previewObject);
                pos.y = -bottomOffset;
            }

            if (!snapEnabled)
            {
                pos.y += heightOffset;
            }

            previewObject.transform.position = pos;
        }

        bool canPlace = CanPlace(previewObject.transform.position);
        SetPreviewColor(canPlace);

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            Instantiate(
                placeableObjects[currentIndex],
                previewObject.transform.position,
                previewObject.transform.rotation
            );

            ClearSnapLock();
        }
    }

    // ==================================================
    // 🔥 핵심：スナップ優先ロジック（コーナー優先）
    // ==================================================
    (bool found, SnapPoint my, SnapPoint target) FindBestSnap()
    {
        SnapPoint[] myPoints = previewObject.GetComponentsInChildren<SnapPoint>();
        SnapPoint[] all = FindObjectsOfType<SnapPoint>();

        float bestScore = float.MaxValue;
        SnapPoint bestMy = null;
        SnapPoint bestTarget = null;

        foreach (var m in myPoints)
        {
            foreach (var t in all)
            {
                if (t.transform.IsChildOf(previewObject.transform)) continue;
                if (t.occupied) continue;

                float dist = Vector3.Distance(m.transform.position, t.transform.position);

                if (dist > snapBreakDistance) continue;

                // ★コーナー優先（中心より強い）
                float score = dist;

                if (m.isCorner && t.isCorner)
                    score *= 0.5f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestMy = m;
                    bestTarget = t;
                }
            }
        }

        return (bestMy != null, bestMy, bestTarget);
    }

    float GetBottomOffset(GameObject obj)
    {
        Renderer r = obj.GetComponentInChildren<Renderer>();
        return r.bounds.min.y - obj.transform.position.y;
    }

    void ClearSnapLock()
    {
        lockedMySnap = null;
        lockedTargetSnap = null;
    }

    bool CanPlace(Vector3 pos)
    {
        return true;
    }

    void ChangeObject(int index)
    {
        currentIndex = index;
        Destroy(previewObject);
        SpawnPreview();
        ClearSnapLock();
    }

    void SpawnPreview()
    {
        previewObject = Instantiate(placeableObjects[currentIndex]);
        SetTransparent(previewObject);

        foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    void SetTransparent(GameObject obj)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            Material mat = new Material(r.material);
            mat.color = new Color(1f, 1f, 1f, 0.5f);
            r.material = mat;
        }
    }

    void SetPreviewColor(bool canPlace)
    {
        foreach (Renderer r in previewObject.GetComponentsInChildren<Renderer>())
        {
            r.material.color = canPlace
                ? new Color(0f, 1f, 0f, 0.5f)
                : new Color(1f, 0f, 0f, 0.5f);
        }
    }
}