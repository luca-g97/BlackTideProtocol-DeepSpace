using KBCore.Refs;

using UnityEngine;

public class Credits : ValidatedMonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Scroll speed in pixels per second.")]
    public float scrollSpeed = 100f;

    [SerializeField, Self] private RectTransform _rectTransform;
    private RectTransform _parentRectTransform;

    private void Awake()
    {
        // Grab the parent RectTransform to act as our screen/boundary reference
        if (transform.parent)
        {
            _parentRectTransform = transform.parent.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("EndlessCreditsScroll requires a parent RectTransform to calculate boundaries.");
            enabled = false;
        }
    }
    
    private void OnEnable()
    {
        Reset();
    }

    private void Update()
    {
        // 1. Move the RectTransform upwards
        _rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // We fetch heights dynamically in Update in case you are using a ContentSizeFitter 
        // that changes the RectTransform height at runtime.
        float creditsHeight = _rectTransform.rect.height;
        float parentHeight = _parentRectTransform.rect.height;

        // 2. Check if the bottom edge has passed the top of the parent.
        // With a Top-Center anchor/pivot, Y=0 means top edges align. 
        // Therefore, Y = creditsHeight means the bottom edge is exactly at the parent's top edge.
        if (_rectTransform.anchoredPosition.y >= creditsHeight)
        {
            // Calculate how far past the boundary we went this frame to prevent stuttering
            float overshoot = _rectTransform.anchoredPosition.y - creditsHeight;

            // 3. Snap back down.
            // Y = -parentHeight places the top of the credits exactly at the bottom of the parent.
            float resetYPosition = -parentHeight + overshoot;
            
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, resetYPosition);
        }
    }
    
    public void Reset()
    {
        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, -_parentRectTransform.rect.height);
    }
}