using UnityEngine;
// Or TMPro if you use TextMeshPro
using System.IO;
using System.Collections.Generic;
using KBCore.Refs;
using TMPro;

[System.Serializable]
public class OilFactsData
{
    public List<string> facts;
}

public class OilFactDisplay : ValidatedMonoBehaviour
{
    [SerializeField, Self] private TMP_Text factText;

    private void Start()
    {
        factText.text = OilFactRoller.Instance.GetRandomFact();
    }
}