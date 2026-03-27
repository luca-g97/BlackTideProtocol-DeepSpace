using PrimeTween;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    [SerializeField] private GameObject _normalModel;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _ghostModel;

    [SerializeField] private float _ghostMoveZ = 1f; // Distance to move the ghost model along Z axis

    private Sequence _currentSequence;

    private int _numberOfCollisions;

    private bool _isShuttingDown;

    private void OnApplicationQuit()
    {
        _isShuttingDown = true;
    }

    private void OnDisable()
    {
        if (!gameObject.scene.isLoaded) _isShuttingDown = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ventil") || other.CompareTag("Obstacle"))
        {
            _numberOfCollisions++;
            // Switch to ghost model
            ShowGhostSequence();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_isShuttingDown) return;
        if (!gameObject.activeInHierarchy) return;

        if (other.CompareTag("Ventil") || other.CompareTag("Obstacle"))
        {
            _numberOfCollisions--;

            if (_numberOfCollisions <= 0)
            {
                HideGhostSequence();
            }
        }
    }

    private void ShowGhostSequence()
    {
        _normalModel.SetActive(false);
        _canvas.enabled = false;
        _ghostModel.SetActive(true);

        _ghostModel.transform.localPosition = _normalModel.transform.localPosition;

        _currentSequence.Kill();
        _currentSequence = DOTween.Sequence()
            .Append(_ghostModel.transform.DOLocalMoveZ(_ghostMoveZ, 0.5f).SetEase(Ease.OutCubic));
    }

    private void HideGhostSequence()
    {
        if (!_normalModel || !_ghostModel || !_canvas) return;

        _normalModel.SetActive(true);
        _ghostModel.SetActive(false);
        _canvas.enabled = true;

        _normalModel.transform.localPosition = _ghostModel.transform.localPosition;

        _currentSequence.Kill();
        _currentSequence = DOTween.Sequence()
            .Append(_normalModel.transform.DOLocalMoveZ(0f, 0.25f).SetEase(Ease.OutBack));
    }

    private void OnDestroy()
    {
        _currentSequence.Kill();
    }
}