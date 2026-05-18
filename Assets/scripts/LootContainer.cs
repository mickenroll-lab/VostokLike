using UnityEngine;
using System.Collections.Generic;

public class LootContainer : MonoBehaviour
{
    public ContainerData containerData;
    public BoxContainer boxContainer;
    public Transform interactPoint;

    private Dictionary<string, int> contents = new Dictionary<string, int>();
    private bool isGenerated = false;
    private string guaranteedWeapon = "";
    private int guaranteedWeaponAmmo = 0;
    private string guaranteedMagazine = "";
    private int guaranteedMagazineAmmo = 0;

    public void Interact()
    {
        Debug.Log($"LootContainer.Interact called isGenerated={isGenerated} boxContainer={(boxContainer != null ? boxContainer.gameObject.name : "null")}");
        if (boxContainer != null && boxContainer.IsOpen)
        {
            Debug.Log("LootContainer.Interact: boxContainer is already open, closing.");
            boxContainer.CloseBox();
            return;
        }
        // �ȉ���������
        // ����C���^���N�g���ɒ��I
        if (!isGenerated)
        {
            GenerateLoot();
            isGenerated = true;
        }

        if (containerData == null)
        {
            Debug.LogWarning("LootContainer.Interact: containerData is null, cannot open container.");
            return;
        }

        Debug.Log($"LootContainer.Interact: calling OpenContainer contentsCount={contents.Count} width={containerData.gridWidth} height={containerData.gridHeight}");
        if (boxContainer == null)
            boxContainer = FindObjectOfType<BoxContainer>();
        if (boxContainer == null)
        {
            Debug.LogWarning("LootContainer.Interact: boxContainer reference is null.");
            return;
        }
        boxContainer.OpenContainer(contents, containerData.gridWidth, containerData.gridHeight, this);
        if (guaranteedWeapon != "")
            boxContainer.AddWeaponToBox(guaranteedWeapon, guaranteedWeaponAmmo);
        if (guaranteedMagazine != "")
            boxContainer.AddMagazineToBox(guaranteedMagazine, guaranteedMagazineAmmo);
    }

    void GenerateLoot()
    {
        contents.Clear();
        if (containerData == null || containerData.lootTable == null)
        {
            Debug.LogWarning("LootContainer.GenerateLoot: containerData or lootTable is null.");
            return;
        }

        // �󔻒�
        float emptyRoll = Random.Range(0f, 100f);
        Debug.Log($"LootContainer.GenerateLoot emptyRoll={emptyRoll} emptyChance={containerData.emptyChance}");
        if (emptyRoll <= containerData.emptyChance)
        {
            Debug.Log("LootContainer.GenerateLoot: generated empty container.");
            return;
        }

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

    public void RemoveFromContents(string itemName, int amount)
    {
        if (itemName == guaranteedWeapon)
        {
            guaranteedWeapon = "";
            guaranteedWeaponAmmo = 0;
            return;
        }
        if (itemName == guaranteedMagazine)
        {
            guaranteedMagazine = "";
            guaranteedMagazineAmmo = 0;
            return;
        }
        if (!contents.ContainsKey(itemName)) return;
        contents[itemName] -= amount;
        if (contents[itemName] <= 0)
            contents.Remove(itemName);
    }

    public void SetGuaranteedWeapon(string weaponName, int ammo)
    {
        guaranteedWeapon = weaponName;
        guaranteedWeaponAmmo = ammo;
    }

    public void SetGuaranteedMagazine(string magazineName, int ammo)
    {
        guaranteedMagazine = magazineName;
        guaranteedMagazineAmmo = ammo;
    }

    public void ResetContainer()
    {
        isGenerated = false;
        contents.Clear();
        guaranteedWeapon = "";
        guaranteedWeaponAmmo = 0;
        guaranteedMagazine = "";
        guaranteedMagazineAmmo = 0;
    }
}