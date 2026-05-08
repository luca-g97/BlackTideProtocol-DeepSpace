using System;
using Seb.Fluid2D.Simulation;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class MonitorMixingChange : MonoBehaviour
{
    [SerializeField] private float colorMixingTimer = 5;
    [SerializeField] private TextMeshProUGUI[] timerTexts;
    [SerializeField] private Image successImage;
    [SerializeField] private Image previewImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private Sprite threeColorImage;
    [SerializeField] private Sprite sixColorImage;

    [SerializeField] private Animator _primaryFlareCrateAnimator;
    [SerializeField] private Animator _secondaryFlareCrateAnimator;
    [SerializeField] private VisualEffect[] _primaryFlareEffects;
    [SerializeField] private VisualEffect[] _secondaryFlareEffects;
    [SerializeField] private AudioSource _primaryFlareAudioSource;
    [SerializeField] private AudioSource _secondaryFlareAudioSource;
    [SerializeField] private AudioSource _successAudioSource;
    [SerializeField] private AudioClip _successAudioClip;
    
    [SerializeField] private float _successImageAnimationDuration = 0.5f;
    [SerializeField] private float _successImageAnimationScale = 5f;
    [SerializeField] private float _crateAnimationDuration = 0.5f;
    [SerializeField] private float _crateAnimationDelay = 0.25f;
        
    private float _internalColorMixingTimer;

    private Collider2D _activatingPlayer;
    private Coroutine _colorMixingCR;
    private FluidSim2D _fluidSim;
    private MissionTracker _missionTracker;
    private BoxCollider2D _colorMixingCollider;
    
    private Sequence _successImageSequence;
    private Sequence _currentCrateSequence;

    private static readonly int FlaresUp = Animator.StringToHash("FlaresUp");
    private static readonly int FlaresDown = Animator.StringToHash("FlaresDown");
    
    // Temp
    [SerializeField] private GameObject _colorMixingCanvas;
    [SerializeField] private GameObject _colorMixingCrate1;
    [SerializeField] private GameObject _colorMixingCrate2;

    private void Awake()
    {
        _colorMixingCollider = GetComponent<BoxCollider2D>();
        _missionTracker = FindFirstObjectByType<MissionTracker>();
    }

    private void OnEnable()
    {
        _missionTracker.OnMissionOver += OnMissionOverHandler;
    }
    
    private void OnDisable()
    {
        _missionTracker.OnMissionOver -= OnMissionOverHandler;
    }

    private void Start()
    {
        SetColorMixing(false);
        UpdateTimerTexts(string.Empty);
        fillImage.fillAmount = 0f;
        successImage.transform.localScale = Vector3.zero;
    }

    private void OnTriggerEnter2D(Collider2D player)
    {
        if (!_fluidSim)
        {
            _fluidSim = FindFirstObjectByType<FluidSim2D>();
        }

        if (player.gameObject.name.Contains("Ghost") && player.transform.parent.parent.GetComponent<FluidObstacle>() &&
            player.transform.parent.parent.GetComponent<PlayerDirectionTracker>() && _activatingPlayer == null)
        {
            _activatingPlayer = player;
            _colorMixingCR = StartCoroutine(ColorMixingCR());

            StartPrimaryFlares();

            if (_fluidSim.colorMixingActivated)
            {
                StartSecondaryFlares();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D player)
    {
        if (player == _activatingPlayer)
        {
            _activatingPlayer = null;
            if (_colorMixingCR != null)
            {
                StopCoroutine(_colorMixingCR);
                _colorMixingCR = null;
                UpdateTimerTexts(string.Empty);
                fillImage.fillAmount = 0;

                StopPrimaryFlares();
                StopSecondaryFlares();
            }
        }
    }

    private IEnumerator ColorMixingCR()
    {
        _internalColorMixingTimer = colorMixingTimer;
        while (_internalColorMixingTimer >= 0.0f)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            _internalColorMixingTimer -= 0.1f;
            UpdateTimerTexts(_internalColorMixingTimer.ToString("0"));
            fillImage.fillAmount = 1 - _internalColorMixingTimer / colorMixingTimer;
        }

        _fluidSim.ToggleColorMixing();
        successImage.sprite = _fluidSim.colorMixingActivated ? threeColorImage : sixColorImage;
        previewImage.sprite = _fluidSim.colorMixingActivated ? sixColorImage : threeColorImage;
        UpdateTimerTexts(string.Empty);
        fillImage.fillAmount = 0;

        StopPrimaryFlares();
        StopSecondaryFlares();
        AnimateSuccessImage();
        
        _successAudioSource.PlayOneShot(_successAudioClip);
    }

    private void StartPrimaryFlares()
    {
        foreach (VisualEffect flare in _primaryFlareEffects)
        {
            if (flare) flare.Play();
        }

        _primaryFlareCrateAnimator.ResetTrigger(FlaresDown);
        _primaryFlareCrateAnimator.SetTrigger(FlaresUp);
        _primaryFlareAudioSource.Play();
    }

    private void StartSecondaryFlares()
    {
        foreach (VisualEffect flare in _secondaryFlareEffects)
        {
            if (flare) flare.Play();
        }

        _secondaryFlareCrateAnimator.ResetTrigger(FlaresDown);
        _secondaryFlareCrateAnimator.SetTrigger(FlaresUp);
        _secondaryFlareAudioSource.Play();
    }

    private void StopPrimaryFlares()
    {
        foreach (VisualEffect flare in _primaryFlareEffects)
        {
            if (flare) flare.Stop();
        }

        _primaryFlareCrateAnimator.SetTrigger(FlaresDown);
        _primaryFlareAudioSource.Stop();
    }

    private void StopSecondaryFlares()
    {
        foreach (VisualEffect flare in _secondaryFlareEffects)
        {
            if (flare) flare.Stop();
        }

        _secondaryFlareCrateAnimator.SetTrigger(FlaresDown);
        _secondaryFlareAudioSource.Stop();
    }

    private void UpdateTimerTexts(string text)
    {
        foreach (TextMeshProUGUI timerText in timerTexts)
        {
            timerText.text = text;
        }
    }

    private void AnimateSuccessImage()
    {
        _successImageSequence.Stop();
        successImage.transform.localScale = Vector3.one;
        successImage.transform.eulerAngles = Vector3.zero;
        successImage.color = Color.white;
        
        _successImageSequence = Sequence.Create()
            .Chain(Tween.Scale(successImage.transform, Vector3.one * _successImageAnimationScale, _successImageAnimationDuration).SetEase(Ease.OutCubic))
            .Group(Tween.Alpha(successImage, 0f, _successImageAnimationDuration).SetEase(Ease.Linear))
            .Group(Tween.Rotation(successImage.transform, Quaternion.Euler(0, 0, 180), _successImageAnimationDuration).SetEase(Ease.Linear));
    }

    private void OnMissionOverHandler()
    {
        AnimateCrates();
        SetColorMixing(true);
    }
    
    private void SetColorMixing(bool enable)
    {
        _colorMixingCollider.enabled = enable;
        
        _colorMixingCrate1.SetActive(enable);
        _colorMixingCrate2.SetActive(enable);
        _colorMixingCanvas.SetActive(enable);
    }

    private void AnimateCrates()
    {
        Transform primaryCrateTransform = _colorMixingCrate1.transform;
        Transform secondaryCrateTransform = _colorMixingCrate2.transform;
        Transform canvasTransform = _colorMixingCanvas.transform;
        
        primaryCrateTransform.localScale = Vector3.zero;
        secondaryCrateTransform.localScale = Vector3.zero;
        canvasTransform.localScale = Vector3.zero;
        
        _currentCrateSequence.Stop();
        
        _currentCrateSequence = Sequence.Create()
            .Chain(Tween.Scale(primaryCrateTransform, Vector3.one, 0.5f).SetEase(Ease.OutBack))
            .Insert(_crateAnimationDelay, Tween.Scale(secondaryCrateTransform, Vector3.one, 0.5f).SetEase(Ease.OutBack))
            .Insert(_crateAnimationDelay * 2, Tween.Scale(canvasTransform, Vector3.one, 0.5f).SetEase(Ease.OutBack));
    }
}