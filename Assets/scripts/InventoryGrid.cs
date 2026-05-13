using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    public int width = 10;
    public int height = 12;

    // �O���b�h�f�[�^�F�e�Z���ɉ��������Ă��邩�inull�Ȃ��j
    private string[,] grid;

    public void Initialize()
    {
        grid = new string[width, height];
    }

    // �A�C�e����z�u�ł��邩�m�F
    public bool CanPlace(int x, int y, int itemWidth, int itemHeight)
    {
        if (grid == null) Initialize();
        if (x + itemWidth > width || y + itemHeight > height) return false;

        for (int dx = 0; dx < itemWidth; dx++)
        {
            for (int dy = 0; dy < itemHeight; dy++)
            {
                if (grid[x + dx, y + dy] != null) return false;
            }
        }
        return true;
    }

    // �A�C�e����z�u
    public bool PlaceItem(string itemId, int x, int y, int itemWidth, int itemHeight)
    {
        if (!CanPlace(x, y, itemWidth, itemHeight)) return false;

        for (int dx = 0; dx < itemWidth; dx++)
        {
            for (int dy = 0; dy < itemHeight; dy++)
            {
                grid[x + dx, y + dy] = itemId;
            }
        }
        return true;
    }

    // �A�C�e�����폜
    public void RemoveItem(string itemId)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == itemId)
                    grid[x, y] = null;
            }
        }
    }

    public void Clear()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = null;
    }

    // �󂫃X�y�[�X����������
    public bool FindFreeSpace(int itemWidth, int itemHeight, out int foundX, out int foundY)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (CanPlace(x, y, itemWidth, itemHeight))
                {
                    foundX = x;
                    foundY = y;
                    return true;
                }
            }
        }
        foundX = -1;
        foundY = -1;
        return false;
    }
}