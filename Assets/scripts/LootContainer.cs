using UnityEngine;
using System.Collections.Generic;

public class LootContainer : MonoBehaviour
{
    public ContainerData containerData;
    public BoxContainer boxContainer;
    public Transform interactPoint;

    private Dictionary<string, int> contents = new Dictionary<string, int>();
    private bool isGenerated = false;

    public void Interact()
    {
        Debug.Log("Interact呼ばれた isGenerated=" + isGenerated);
        // 以下既存処理
        // 初回インタラクト時に抽選
        if (!isGenerated)
        {
            GenerateLoot();
            isGenerated = true;
        }

        // BoxContainerにコンテナ情報を渡して開く
        boxContainer.OpenContainer(contents, containerData.gridWidth, containerData.gridHeight);
    }

    void GenerateLoot()
    {
        contents.Clear();
        if (containerData == null || containerData.lootTable == null) return;

        // 空判定
        float emptyRoll = Random.Range(0f, 100f);
        if (emptyRoll <= containerData.emptyChance) return;

        foreach (var entry in containerData.lootTable.entries)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= entry.chance)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                if (contents.ContainsKey(entry.itemName))
                    contents[entry.itemName] += amount;
                else
                    contents[entry.itemName] = amount;
            }
        }
    }

    public void ResetContainer()
    {
        isGenerated = false;
        contents.Clear();
    }
}