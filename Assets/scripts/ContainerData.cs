using UnityEngine;

[CreateAssetMenu(fileName = "ContainerData", menuName = "VostokLike/ContainerData")]
public class ContainerData : ScriptableObject
{
    public int gridWidth = 4;
    public int gridHeight = 4;
    public LootTable lootTable;
    [Range(0, 100)]
    public float emptyChance = 30f; // ãÛÇÃämó¶Åi%Åj
}