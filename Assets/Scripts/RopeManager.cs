using Seb.Fluid2D.Simulation;
using System.Collections.Generic;
using UnityEngine;

public class RopeManager : MonoBehaviour
{
    public static RopeManager Instance { get; private set; }
    private FluidSim2D _fluidSim2D;
    private bool preventRopesUpdate = true;

    [Tooltip("Prefab that contains a RopeInstance component and the rope visuals (Rope, RopeMesh, MeshRenderer).")]
    public GameObject ropePrefab;

    // One entry per unordered pair
    private readonly Dictionary<ConnectionKey, RopeInstance> _connections = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _fluidSim2D = GameObject.FindFirstObjectByType<FluidSim2D>();
    }

    public void Update()
    {
        if (!preventRopesUpdate && _fluidSim2D != null && !_fluidSim2D.colorMixingActivated)
        {
            preventRopesUpdate = true;

            foreach (ConnectionKey connection in _connections.Keys)
            {
                DisconnectAllFor(connection.A, false);
            }

            _connections.Clear();
        }
        else if (preventRopesUpdate && _fluidSim2D != null && _fluidSim2D.colorMixingActivated)
        {
            preventRopesUpdate = false;
        }
    }

    public bool AreConnected(PlayerRopeAnchor a, PlayerRopeAnchor b)
    {
        if (a == null || b == null) return false;
        return _connections.ContainsKey(new ConnectionKey(a, b));
    }

    /// <summary>
    /// Create a rope between the two anchors (if one doesn't already exist).
    /// The rope prefab will be parented to the 'owner' (the anchor with the smaller InstanceID).
    /// </summary>
    public void Connect(PlayerRopeAnchor a, PlayerRopeAnchor b)
    {
        // prevent connecting boats with the same player color
        if (a == null || b == null || (a.GetPlayerColor() == b.GetPlayerColor()) || preventRopesUpdate)
            return;

        if (ropePrefab == null)
        {
            Debug.LogWarning("RopeManager: ropePrefab not assigned.");
            return;
        }

        var key = new ConnectionKey(a, b);
        if (_connections.ContainsKey(key))
            return;

        // Owner is key.A (smaller instance id)
        var owner = key.A;
        var other = key.B;

        var go = Instantiate(ropePrefab, owner.transform.position, Quaternion.identity, owner.transform);
        var ropeInstance = go.GetComponent<RopeInstance>();
        if (ropeInstance == null)
        {
            Debug.LogError("RopeManager: ropePrefab must contain a RopeInstance component.");
            Destroy(go);
            return;
        }

        var color = DetermineMixedColor(owner, other);
        ropeInstance.Initialize(owner, other, color);

        _connections.Add(key, ropeInstance);
    }


    public void Disconnect(PlayerRopeAnchor a, PlayerRopeAnchor b, bool removeEntry = true)
    {
        if (a == null || b == null)
            return;

        var key = new ConnectionKey(a, b);
        if (!_connections.TryGetValue(key, out var ropeInstance))
            return;

        if (removeEntry)
        {
            _connections.Remove(key);
        }

        ropeInstance.DestroyRope();
    }

    /// <summary>
    /// Remove all connections for the given anchor (called e.g. on disable).
    /// </summary>
    public void DisconnectAllFor(PlayerRopeAnchor anchor, bool removeEntry = true)
    {
        if (anchor == null) return;

        // collect keys to remove to avoid modifying dictionary while iterating
        var toRemove = new List<ConnectionKey>();
        foreach (var kv in _connections)
        {
            if (kv.Key.Contains(anchor))
                toRemove.Add(kv.Key);
        }

        foreach (var key in toRemove)
        {
            var other = key.A == anchor ? key.B : key.A;
            Disconnect(anchor, other, removeEntry);
        }
    }

    private Color DetermineMixedColor(PlayerRopeAnchor a, PlayerRopeAnchor b)
    {
        var pa = a.GetComponentInParent<PlayerColor>();
        var pb = b.GetComponentInParent<PlayerColor>();
        Color ca = pa != null ? pa.currentColor : Color.white;
        Color cb = pb != null ? pb.currentColor : Color.white;
        Color mixed = (ca + cb) * 0.5f;

        // keep your original green fix
        if (mixed == ColorPalette.colorPalette[5])
            mixed = ColorPalette.actualGreen;

        return mixed;
    }

    // ---------- internal convenience struct to be used as dictionary key ----------
    private struct ConnectionKey
    {
        public readonly PlayerRopeAnchor A;
        public readonly PlayerRopeAnchor B;

        public ConnectionKey(PlayerRopeAnchor a, PlayerRopeAnchor b)
        {
            // normalize ordering so (a,b) == (b,a)
            if (a == null || b == null)
            {
                A = a; B = b;
                return;
            }

            if (a.GetInstanceID() <= b.GetInstanceID())
            {
                A = a; B = b;
            }
            else
            {
                A = b; B = a;
            }
        }

        public bool Equals(ConnectionKey other) => A == other.A && B == other.B;
        public override bool Equals(object obj) => obj is ConnectionKey other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                int hashA = A != null ? A.GetInstanceID() : 0;
                int hashB = B != null ? B.GetInstanceID() : 0;
                return (hashA * 397) ^ hashB;
            }
        }

        public bool Contains(PlayerRopeAnchor anchor) => anchor == A || anchor == B;
    }
}
