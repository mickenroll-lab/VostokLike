using UnityEngine;
using System.Collections.Generic;

public class ItemBox : MonoBehaviour
{
    public Dictionary<string, int> contents = new Dictionary<string, int>();

    void Start()
    {
        contents["5.56x18mm"] = 30;
        contents["MedKit"] = 1;
    }
}