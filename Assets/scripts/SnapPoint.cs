using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    public enum SnapType
    {
        Wall,
        Floor,
        Pillar,
        Cube
    }

    public enum SnapDir
    {
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back
    }

    [Header("Snap Settings")]
    public SnapType type;
    public SnapDir direction;

    [Header("Priority")]
    public bool isCorner = false;
    public bool occupied = false;

    void OnDrawGizmos()
    {
        Gizmos.color = isCorner ? Color.yellow : Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.05f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 0.2f);
    }
}