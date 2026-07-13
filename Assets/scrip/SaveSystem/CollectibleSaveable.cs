using System;
using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public sealed class CollectibleSaveable : MonoBehaviour, ISaveable
{
    [Serializable]
    private sealed class State
    {
        public bool collected;
    }

    [SerializeField] private bool collected;

    public bool IsCollected => collected;

    public void Collect()
    {
        if (collected)
            return;

        collected = true;
        ApplyCollectedVisual();
    }

    private void Awake()
    {
        ApplyCollectedVisual();
    }

    private void ApplyCollectedVisual()
    {
        if (!collected)
            return;

        if (TryGetComponent<Collider2D>(out var col2d))
            col2d.enabled = false;

        if (TryGetComponent<Collider>(out var col3d))
            col3d.enabled = false;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
            r.enabled = false;
    }

    public object CaptureState()
    {
        return new State { collected = collected };
    }

    public void RestoreState(object state)
    {
        if (state is not State s)
            return;

        collected = s.collected;
        ApplyCollectedVisual();
    }
}

