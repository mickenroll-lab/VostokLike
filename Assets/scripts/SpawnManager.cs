using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform spawnPointSafe;   // セーフゾーン内
    public Transform spawnPointField;  // セーフゾーン外
    public GameObject player;

    public void SpawnAtField()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = spawnPointField.position;
        player.transform.rotation = spawnPointField.rotation;
        if (cc != null) cc.enabled = true;
    }

    public void SpawnAtSafe()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = spawnPointSafe.position;
        player.transform.rotation = spawnPointSafe.rotation;
        if (cc != null) cc.enabled = true;
    }
    void Start()
    {
        SpawnAtSafe(); // デフォルトはセーフゾーン内スタート
    }
}