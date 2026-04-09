using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityUtils;

public class OilFactRoller : PersistentSingleton<OilFactRoller>
{
    private OilFactsData _oilFacts;
    private int _currentFactIndex;
    
    private void OnEnable()
    {
        LoadFactsFromJson();
        if (_oilFacts != null && _oilFacts.facts.Count > 0)
        {
            _currentFactIndex = Random.Range(0, _oilFacts.facts.Count);
        }
    }

    private void LoadFactsFromJson()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "OilFacts.json");

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            _oilFacts = JsonUtility.FromJson<OilFactsData>(jsonContent);
        }
        else
        {
            Debug.LogError("OilFacts.json not found at: " + filePath);
            _oilFacts = new OilFactsData { facts = new List<string>() };
        }
    }
    
    public string GetRandomFact()
    {
        if (_oilFacts != null && _oilFacts.facts.Count > 0)
        {
            _currentFactIndex = (_currentFactIndex + 1) % _oilFacts.facts.Count;
            
            return _oilFacts.facts[_currentFactIndex];
        }
        
        return "No facts available.";
    }
}