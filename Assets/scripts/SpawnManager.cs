using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform spawnPointSafe;   // セーフゾーン内
    public Transform spawnPointField;  // セーフゾーン外
    public GameObject player;

    public void SpawnAtField()
    {
        if (player == null || !player.activeInHierarchy)
            player = GameObject.FindWithTag("Player");

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = spawnPointField.position;
        player.transform.rotation = spawnPointField.rotation;
        if (cc != null) cc.enabled = true;
    }

    public void SpawnAtSafe()
    {
        if (player == null || !player.activeInHierarchy)
            player = GameObject.FindWithTag("Player");

        Debug.Log("SpawnAtSafe実行 player：" + player.GetInstanceID());
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = spawnPointSafe.position;
        player.transform.rotation = spawnPointSafe.rotation;
        if (cc != null) cc.enabled = true;
        Debug.Log("移動後position：" + player.transform.position);
    }
    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");

        Debug.Log("SpawnManager Start player：" + player);
        SpawnAtSafe();
        Debug.Log("SpawnAtSafe呼んだ position：" + spawnPointSafe.position);
    }
}