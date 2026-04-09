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
    
    [SerializeField] private float _successImageAnimationDuration = 0.5f;
    [SerializeField] private float _successImageAnimationScale = 5f;

    private float internalColorMixingTimer;

    private Collider2D activatingPlayer;
    private Coroutine colorMixingCR;
    private FluidSim2D fluidSim;

    private Sequence _successImageSequence;

    private static readonly int FlaresUp = Animator.StringToHash("FlaresUp");
    private static readonly int FlaresDown = Animator.StringToHash("FlaresDown");

    private void Start()
    {
        UpdateTimerTexts(string.Empty);
        fillImage.fillAmount = 0f;
        successImage.transform.localScale = Vector3.zero;
    }

    private void OnTriggerEnter2D(Collider2D player)
    {
        if (!fluidSim)
        {
            fluidSim = FindFirstObjectByType<FluidSim2D>();
        }

        Debug.Log(player.gameObject.name);

        if (player.gameObject.name.Contains("Ghost") && player.transform.parent.parent.GetComponent<FluidObstacle>() &&
            player.transform.parent.parent.GetComponent<PlayerDirectionTracker>() && activatingPlayer == null)
        {
            activatingPlayer = player;
            colorMixingCR = StartCoroutine(ColorMixingCR());

            StartPrimaryFlares();

            if (fluidSim.colorMixingActivated)
            {
                StartSecondaryFlares();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D player)
    {
        if (player == activatingPlayer)
        {
            activatingPlayer = null;
            if (colorMixingCR != null)
            {
                StopCoroutine(colorMixingCR);
                colorMixingCR = null;
                UpdateTimerTexts(string.Empty);
                fillImage.fillAmount = 0;

                StopPrimaryFlares();
                StopSecondaryFlares();
            }
        }
    }

    private IEnumerator ColorMixingCR()
    {
        internalColorMixingTimer = colorMixingTimer;
        while (internalColorMixingTimer >= 0.0f)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            internalColorMixingTimer -= 0.1f;
            UpdateTimerTexts(internalColorMixingTimer.ToString("0"));
            fillImage.fillAmount = 1 - internalColorMixingTimer / colorMixingTimer;
        }

        fluidSim.ToggleColorMixing();
        successImage.sprite = fluidSim.colorMixingActivated ? threeColorImage : sixColorImage;
        previewImage.sprite = fluidSim.colorMixingActivated ? sixColorImage : threeColorImage;
        UpdateTimerTexts(string.Empty);
        fillImage.fillAmount = 0;

        StopPrimaryFlares();
        StopSecondaryFlares();
        AnimateSuccessImage();
    }

    private void StartPrimaryFlares()
    {
        foreach (VisualEffect flare in _primaryFlareEffects)
        {
            flare.Play();
        }

        _primaryFlareCrateAnimator.ResetTrigger(FlaresDown);
        _primaryFlareCrateAnimator.SetTrigger(FlaresUp);
    }

    private void StartSecondaryFlares()
    {
        foreach (VisualEffect flare in _secondaryFlareEffects)
        {
            flare.Play();
        }

        _secondaryFlareCrateAnimator.ResetTrigger(FlaresDown);
        _secondaryFlareCrateAnimator.SetTrigger(FlaresUp);
    }

    private void StopPrimaryFlares()
    {
        foreach (VisualEffect flare in _primaryFlareEffects)
        {
            flare.Stop();
        }

        _primaryFlareCrateAnimator.SetTrigger(FlaresDown);
    }

    private void StopSecondaryFlares()
    {
        foreach (VisualEffect flare in _secondaryFlareEffects)
        {
            flare.Stop();
        }

        _secondaryFlareCrateAnimator.SetTrigger(FlaresDown);
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
}