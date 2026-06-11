using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateManager : MonoBehaviour
{
    public static GateManager Instance { get; private set; }

    [Header("Tham chiếu")]
    public AncientGateController ancientGate;
    public GateProgressUI progressUI;

    [Header("Cấu hình")]
    [Tooltip("Tổng số phiến đá cần kích hoạt. Mặc định 8.")]
    public int requiredPedestalCount = 8;

    public int ActivatedPedestalCount { get; private set; }
    public int TotalPedestalCount => _pedestals.Count > 0 ? _pedestals.Count : requiredPedestalCount;

    public event Action<int, int> OnProgressChanged;
    public event Action OnAllPedestalsActivated;

    private readonly List<GemPedestal> _pedestals = new List<GemPedestal>();
    private bool _gateOpenRequested;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (ancientGate == null)
            ancientGate = FindAnyObjectByType<AncientGateController>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private IEnumerator Start()
    {
        yield return null;
        RecalculateProgress();
    }

    public void RegisterPedestal(GemPedestal pedestal)
    {
        if (pedestal == null || _pedestals.Contains(pedestal))
            return;

        _pedestals.Add(pedestal);
    }

    public void UnregisterPedestal(GemPedestal pedestal)
    {
        if (pedestal == null)
            return;

        _pedestals.Remove(pedestal);
        RecalculateProgress();
    }

    public void RecalculateProgress()
    {
        ActivatedPedestalCount = 0;

        foreach (GemPedestal pedestal in _pedestals)
        {
            if (pedestal != null && pedestal.State == PedestalState.Filled)
                ActivatedPedestalCount++;
        }

        RefreshProgressUI();
    }

    public void OnPedestalActivated(GemPedestal pedestal)
    {
        if (pedestal == null || pedestal.State != PedestalState.Filled)
            return;

        ActivatedPedestalCount++;
        RefreshProgressUI();
        OnProgressChanged?.Invoke(ActivatedPedestalCount, TotalPedestalCount);

        int targetCount = _pedestals.Count > 0
            ? _pedestals.Count
            : requiredPedestalCount;

        if (ActivatedPedestalCount >= targetCount)
            TryOpenGate();
    }

    private void TryOpenGate()
    {
        if (_gateOpenRequested)
            return;

        _gateOpenRequested = true;
        OnAllPedestalsActivated?.Invoke();
        progressUI?.ShowGateOpenedMessage();

        if (ancientGate != null)
            ancientGate.OpenGate();
        else
            Debug.LogWarning("[GateManager] Chưa gán AncientGateController.");
    }

    private void RefreshProgressUI()
    {
        progressUI?.SetProgress(ActivatedPedestalCount, TotalPedestalCount);
    }
}
