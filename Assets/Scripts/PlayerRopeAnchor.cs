using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerRopeAnchor : MonoBehaviour
{
    private PlayerColor _playerColor;
    private bool ropeInitiated = false;

    private void Awake()
    {
        _playerColor = GetComponentInParent<PlayerColor>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerRopeAnchor otherAnchor))
            return;

        RopeManager.Instance?.Connect(this, otherAnchor);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerRopeAnchor otherAnchor))
            return;

        RopeManager.Instance?.Disconnect(this, otherAnchor);
    }

    public Color GetPlayerColor() => _playerColor != null ? _playerColor.currentColor : Color.white;

    private void OnDisable()
    {
        // make sure manager cleans up
        RopeManager.Instance?.DisconnectAllFor(this);
    }

    private void OnDestroy()
    {
        // ensure all connections are cleared when destroyed
        RopeManager.Instance?.DisconnectAllFor(this);
    }
}