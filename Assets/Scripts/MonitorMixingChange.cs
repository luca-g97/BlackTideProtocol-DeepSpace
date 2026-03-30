using Seb.Fluid2D.Simulation;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonitorMixingChange : MonoBehaviour
{
    [SerializeField] private float colorMixingTimer = 5;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite threeColorImage;
    [SerializeField] private Sprite sixColorImage;

    private float internalColorMixingTimer;

    private Collider2D activatingPlayer;
    private Coroutine colorMixingCR;

    private FluidSim2D fluidSim;

    private void OnTriggerEnter2D(Collider2D player)
    {
        if (!fluidSim)
        {
            fluidSim = FindFirstObjectByType<FluidSim2D>();
        }

        Debug.Log(player.gameObject.name);

        if (player.gameObject.name.Contains("Ghost") && player.transform.parent.parent.GetComponent<FluidObstacle>() && player.transform.parent.parent.GetComponent<PlayerDirectionTracker>() && activatingPlayer == null)
        {
            activatingPlayer = player;
            colorMixingCR = StartCoroutine(ColorMixingCR());
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
                timerText.text = string.Empty;
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
            timerText.text = internalColorMixingTimer.ToString("0.0");
        }

        fluidSim.ToggleColorMixing();
        targetImage.sprite = fluidSim.colorMixingActivated ? sixColorImage : threeColorImage;
        timerText.text = string.Empty;
    }
}