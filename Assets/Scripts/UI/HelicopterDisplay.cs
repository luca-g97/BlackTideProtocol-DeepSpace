using System;
using System.Linq;
using DG.Tweening;
using KBCore.Refs;
using Seb.Fluid2D.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelicopterDisplay : ValidatedMonoBehaviour
{
    [SerializeField] private CanvasGroup _clockCanvasGroup;
    [SerializeField] private CanvasGroup _colorModeCanvasGroup;

    [Header("Mission Clock")]
    [SerializeField] private TMP_Text _missionClockText;
    [SerializeField] private TMP_Text _missionOverText;
    [SerializeField] private TMP_Text _missionRestartText;
    [SerializeField, Child] private AudioSource _audioSource;
    [SerializeField] private AudioClip _countdownClip;
    [SerializeField] private AudioClip _missionOverClip;

    [Header("Color Mode")]
    [SerializeField] private RectTransform _colorIconLayout;
    [SerializeField] private TMP_Text _colorModeText;
    [SerializeField] private Color _activatedColorModeTextColor;
    [SerializeField] private Color _deactivatedColorModeTextColor;
    [SerializeField] private float _colorModeDisplayDuration = 2f;
    
    private Image[] _colorIcons;

    private MissionTracker _missionTracker;
    private FluidSim2D _fluidSim;
    
    private Sequence _currentTimerSequence;
    private Sequence _currentColorModeSequence;

    private bool _updateTimeDisplay = true;

    private void Awake()
    {
        _missionTracker = FindFirstObjectByType<MissionTracker>();
        _fluidSim = FindFirstObjectByType<FluidSim2D>();
        _colorIcons = _colorIconLayout.GetComponentsInChildren<Image>();
    }

    private void Start()
    {
        _missionClockText.gameObject.SetActive(true);
        _missionOverText.gameObject.SetActive(false);
        _missionRestartText.gameObject.SetActive(false);
        
        SetupColorIcons();
    }

    private void OnEnable()
    {
        _missionTracker.OnMissionOver += OnMissionOverHandler;
        _missionTracker.OnMissionGraded += OnMissionGradedHandler;
        _missionTracker.OnSecondPassed += OnSecondPassedHandler;
        
        _fluidSim.OnColorMixingModeChanged += OnColorMixingModeChangedHandler;
    }

    private void OnDisable()
    {
        _missionTracker.OnMissionOver -= OnMissionOverHandler;
        _missionTracker.OnMissionGraded -= OnMissionGradedHandler;
        _missionTracker.OnSecondPassed -= OnSecondPassedHandler;
        
        _fluidSim.OnColorMixingModeChanged -= OnColorMixingModeChangedHandler;
    }

    private void OnDestroy()
    {
        _currentTimerSequence?.Kill();
        _currentColorModeSequence?.Kill();
    }

    private void OnMissionGradedHandler(int _)
    {
        _missionClockText.gameObject.SetActive(true);
        _missionRestartText.gameObject.SetActive(true);
        _missionOverText.gameObject.SetActive(false);

        _missionClockText.alpha = 1f;
        _updateTimeDisplay = true;
    }

    private void OnSecondPassedHandler(int _)
    {
        _currentTimerSequence?.Kill();
        _currentTimerSequence = DOTween.Sequence()
            .Append(_missionClockText.DOFade(1, 0f))
            .Append(_missionClockText.DOFade(0.5f, 1f));

        if (_missionTracker.missionIsGraded)
        {
            if (_missionTracker.missionRestartTimeLeft is <= 10 and > 0)
            {
                _audioSource.PlayOneShot(_countdownClip);
            }
        }

        else
        {
            if (_missionTracker.missionRuntimeLeft is <= 10 and > 0)
            {
                _audioSource.PlayOneShot(_countdownClip);
            }
        }
    }

    private void Update()
    {
        if (_updateTimeDisplay)
        {
            DisplayTime(!_missionTracker.missionIsGraded
                ? _missionTracker.missionRuntimeLeft
                : _missionTracker.missionRestartTimeLeft);
        }
    }

    private void OnMissionOverHandler()
    {
        _updateTimeDisplay = false;

        _currentTimerSequence?.Kill();

        BlinkOutSequence(_missionClockText).OnComplete(delegate
        {
            _missionOverText.gameObject.SetActive(true);
            _missionClockText.gameObject.SetActive(false);

            BlinkInSequence(_missionOverText);
        });

        _audioSource.PlayOneShot(_missionOverClip, 2f);
    }

    private static Sequence BlinkOutSequence(Graphic graphic)
    {
        return DOTween.Sequence()
            .Append(graphic.DOFade(0f, 0.1f))
            .Append(graphic.DOFade(1f, 0.1f))
            .Append(graphic.DOFade(0f, 0.1f))
            .Append(graphic.DOFade(1f, 0.1f))
            .Append(graphic.DOFade(0f, 0.1f));
    }

    private static Sequence BlinkInSequence(Graphic graphic)
    {
        return DOTween.Sequence()
            .Append(graphic.DOFade(1f, 0.1f))
            .Append(graphic.DOFade(0f, 0.1f))
            .Append(graphic.DOFade(1f, 0.1f))
            .Append(graphic.DOFade(0f, 0.1f))
            .Append(graphic.DOFade(1f, 0.1f));
    }
    
    private static Sequence BlinkOutSequence(CanvasGroup graphic)
    {
        return DOTween.Sequence()
            .Append(graphic.DOFade(0f, 0.1f))
            .Append(graphic.DOFade(1f, 0.1f))
            .Append(graphic.DOFade(0f, 0.1f))
            .Append(graphic.DOFade(1f, 0.1f))
            .Append(graphic.DOFade(0f, 0.1f));
    }

    private static Sequence BlinkInSequence(CanvasGroup graphic)
    {
        return DOTween.Sequence()
            .Append(graphic.DOFade(1f, 0.1f))
            .Append(graphic.DOFade(0f, 0.1f))
            .Append(graphic.DOFade(1f, 0.1f))
            .Append(graphic.DOFade(0f, 0.1f))
            .Append(graphic.DOFade(1f, 0.1f));
    }
    

    private void DisplayTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        _missionClockText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
    
    private void OnColorMixingModeChangedHandler(bool mixingIsOn)
    {
        DisplayColorModeSwitch(mixingIsOn);
    }

    private void DisplayColorModeSwitch(bool mixingIsOn)
    {
        _currentColorModeSequence?.Kill();
        
        UpdateColorModeIcons(mixingIsOn);
        UpdateColorModeText(mixingIsOn);

        _clockCanvasGroup.alpha = 0f;

        _currentColorModeSequence = DOTween.Sequence()
            .Append(BlinkInSequence(_colorModeCanvasGroup))
            .AppendInterval(_colorModeDisplayDuration)
            .Append(BlinkOutSequence(_colorModeCanvasGroup))
            .Append(BlinkInSequence(_clockCanvasGroup));
    }

    private void SetupColorIcons()
    {
        int[] colorOrder = { 0, 3, 1, 5, 2, 4 };

        for (var i = 0; i < _colorIcons.Length; i++)
        {
            Image colorIcon = _colorIcons[i];
            
            if (i < colorOrder.Length)
            {
                int paletteIndex = colorOrder[i];

                colorIcon.color = paletteIndex == 5 ? ColorPalette.actualGreen : ColorPalette.colorPalette[paletteIndex];
            }

            colorIcon.gameObject.SetActive(false);
        }
    }

    private void UpdateColorModeIcons(bool mixingIsOn)
    {
        int activeIconsCount = _fluidSim.maxPlayerColors;

        foreach (Image colorIcon in _colorIcons)
        {
            colorIcon.gameObject.SetActive(false);
        }
        
        for (int i = 0; i < activeIconsCount; i++)
        {
            int primaryIndex = i;
            
            if (mixingIsOn)
            {
                primaryIndex = i * 2;
            }
            
            _colorIcons[primaryIndex].gameObject.SetActive(true);
        }
    }
    
    private void UpdateColorModeText(bool mixingIsOn)
    {
        _colorModeText.text = mixingIsOn ? "ON" : "OFF";
        
        float textAlpha = _colorModeText.color.a;
        
        Color targetColor = mixingIsOn ? _activatedColorModeTextColor : _deactivatedColorModeTextColor;
        targetColor.a = textAlpha;
        
        _colorModeText.color = targetColor;
    }
}