using System;
using KBCore.Refs;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]

public class ShowCredits : ValidatedMonoBehaviour
{
    [SerializeField] private GameObject _credits;
    [SerializeField] private bool _showCreditsOnStart;
    
    private MissionTracker _missionTracker;

    private void Awake()
    {
        _missionTracker = FindFirstObjectByType<MissionTracker>();
    }

    private void OnEnable()
    {
        if (_missionTracker)
        {
            _missionTracker.OnMissionOver += OnMissionOverHandler;
        }
    }

    private void OnDisable()
    {
        if (_missionTracker)
        {
            _missionTracker.OnMissionOver -= OnMissionOverHandler;
        }
    }

    private void OnMissionOverHandler()
    {
       EnableCredits();
    }

    private void Start()
    {
        _credits.gameObject.SetActive(_showCreditsOnStart);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCredits();
        }
    }
    
    private void ToggleCredits()
    {
        _credits.gameObject.SetActive(!_credits.gameObject.activeSelf);
    }
    
    private void EnableCredits()
    {
        _credits.gameObject.SetActive(true);
    }
    
    private void DisableCredits() 
    {
        _credits.gameObject.SetActive(false);
    }
    
    
}