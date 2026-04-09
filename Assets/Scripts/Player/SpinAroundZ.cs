using UnityEngine;
using UnityEngine.UI;

public class SpinAroundZ : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private float _spinSpeed = 180f;
    [SerializeField] private bool _randomStartRotation = true;

    [Header("Pause Settings")]
    [SerializeField] private GameObject _playerRoot;
    [SerializeField] private float _blinksPerSecond = 2f;

    private Image _imageComponent;
    private float _timer;
    private float _toggleInterval;

    private void Start()
    {
        if (_randomStartRotation)
        {
            float randomZ = Random.Range(0f, 360f);
            transform.Rotate(0f, 0f, randomZ);
        }
        
        if (_playerRoot == null) _playerRoot = transform.root.gameObject;
        
        _imageComponent = GetComponent<Image>();
        if (_imageComponent == null)
        {
            Debug.LogWarning("SpinAroundZ: No Image component found to blink!", this);
        }

        _toggleInterval = 1f / (_blinksPerSecond * 2f);
    }

    private void Update()
    {
        bool isLocked = MonitorPause.Instance != null && MonitorPause.Instance.IsPlayerLocked(_playerRoot);

        if (!isLocked)
        {
            transform.Rotate(0f, 0f, _spinSpeed * Time.deltaTime);
            
            if (_imageComponent != null && !_imageComponent.enabled)
            {
                _imageComponent.enabled = true;
            }
            
            _timer = 0f; 
        }
        else
        {
            if (_imageComponent != null)
            {
                _timer += Time.deltaTime;
                if (_timer >= _toggleInterval)
                {
                    _timer -= _toggleInterval; 
                    _imageComponent.enabled = !_imageComponent.enabled;
                }
            }
        }
    }
}