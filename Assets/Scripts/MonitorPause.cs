using Seb.Fluid2D.Simulation;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonitorPause : MonoBehaviour
{
    public static MonitorPause Instance { get; private set; }
    
    public bool isGamePaused { get; private set; }

    [SerializeField] private float pauseTimer = 3;
    [SerializeField] private TextMeshProUGUI[] timerTexts;
    [SerializeField] private Image previewImage;
    [SerializeField] private Image fillImage;

    private float internalPauseTimer;

    private Collider2D activatingPlayer;
    private Coroutine pauseCR;
    
    private FluidSim2D fluidSim;
    private FluidSim2D_Wall wallSim;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateTimerTexts(string.Empty);
        fillImage.fillAmount = 0f;
    }

    private void OnTriggerEnter2D(Collider2D player)
    {
        if (!fluidSim) fluidSim = FindFirstObjectByType<FluidSim2D>();
        if (!wallSim) wallSim = FindFirstObjectByType<FluidSim2D_Wall>();

        if (player.gameObject.name.Contains("Ghost") && player.transform.parent.parent.GetComponent<FluidObstacle>() &&
            player.transform.parent.parent.GetComponent<PlayerDirectionTracker>() && activatingPlayer == null)
        {
            activatingPlayer = player;
            pauseCR = StartCoroutine(PauseCR());
        }
    }

    private void OnTriggerExit2D(Collider2D player)
    {
        if (player == activatingPlayer)
        {
            activatingPlayer = null;
            if (pauseCR != null)
            {
                StopCoroutine(pauseCR);
                pauseCR = null;
                UpdateTimerTexts(string.Empty);
                fillImage.fillAmount = 0;
            }
            SetGameOnPause(false);
        }
    }

    private IEnumerator PauseCR()
    {
        internalPauseTimer = pauseTimer;
        while (internalPauseTimer >= 0.0f)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            internalPauseTimer -= 0.1f;
            UpdateTimerTexts(internalPauseTimer.ToString("0"));
            fillImage.fillAmount = 1 - internalPauseTimer / pauseTimer;
        }

        SetGameOnPause(true);
        UpdateTimerTexts(string.Empty);
    }

    private void SetGameOnPause(bool isPaused)
    {
        isGamePaused = isPaused;
        
        if (isPaused)
        {
            fluidSim.pauseNextFrame = true;
            wallSim.pauseNextFrame = true;
        }
        else 
        {
            fluidSim.unPauseNextFrame = true; 
            wallSim.unPauseNextFrame = true;
        }
    }
    
    public bool IsPlayerLocked(GameObject playerRoot)
    {
        if (!isGamePaused || activatingPlayer == null) return false;
        GameObject activatingRoot = activatingPlayer.transform.parent.parent.gameObject;
        if (playerRoot == activatingRoot) return false;
        return true;
    }

    private void UpdateTimerTexts(string text)
    {
        foreach (TextMeshProUGUI timerText in timerTexts)
        {
            timerText.text = text;
        }
    }
}