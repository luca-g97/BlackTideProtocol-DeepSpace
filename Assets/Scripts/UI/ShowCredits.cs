using System;
using KBCore.Refs;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]

public class ShowCredits : ValidatedMonoBehaviour
{
    [SerializeField] private GameObject _credits;
    [SerializeField] private bool _showCreditsOnStart;

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
    
    
}