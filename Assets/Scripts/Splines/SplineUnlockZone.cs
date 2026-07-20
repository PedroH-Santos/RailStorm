using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

[RequireComponent(typeof(SphereCollider))]
public class SplineUnlockZone : MonoBehaviour
{
    [Header("Collider desta zona (independente do SplineCollision)")]
    [SerializeField] private float unlockRadius = 3f;

    [Header("Referência ao SplineContainer da cena")]
    [SerializeField] private SplineContainer splineContainer;

    [Header("Totens e Foco")]
    [SerializeField] private JunctionTotemsController totemsController;
    [SerializeField] private FocusDimController focusDim;

    PlayerController _player;
    PlayerStatsAggregator _stats;
    bool _menuOpen;

    List<SplineEntry> _relevantEntries = new();
    List<SplineEntry> _blockedHere = new();

    SplineEntry _selectedEntry;

    bool _stickMovedLeft = false;
    bool _stickMovedRight = false;

    void Awake()
    {
        var col = GetComponent<SphereCollider>();
        col.radius = unlockRadius;
        col.isTrigger = true;
    }

    void Update()
    {
        if (_player == null) return;

        _blockedHere = SplineRuntimeState.Instance
            .GetBlockedEntriesFrom(_relevantEntries)
            .ToList();

        bool hasBlocked = _blockedHere.Count > 0;

        if (hasBlocked && !_menuOpen)
            InteractPromptUI.Instance?.Show();
        else
            InteractPromptUI.Instance?.Hide();

        if (hasBlocked && !_menuOpen && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenMenu();
            return;
        }

        if (_menuOpen && (Keyboard.current.eKey.wasPressedThisFrame
                       || Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            CloseMenu();
            return;
        }

        if (_menuOpen)
            HandleUnlockInput();
    }

    void HandleUnlockInput()
    {
        // Navegação — teclado
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            MoveSelection(-1);
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            MoveSelection(1);

        // Navegação — gamepad (analógico esquerdo, com debounce)
        if (Gamepad.current != null)
        {
            float stickX = Gamepad.current.leftStick.x.ReadValue();

            if (stickX < -0.5f && !_stickMovedLeft)
            {
                MoveSelection(-1);
                _stickMovedLeft = true;
            }
            else if (stickX >= -0.5f)
            {
                _stickMovedLeft = false;
            }

            if (stickX > 0.5f && !_stickMovedRight)
            {
                MoveSelection(1);
                _stickMovedRight = true;
            }
            else if (stickX <= 0.5f)
            {
                _stickMovedRight = false;
            }
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            TryUnlock();

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
            TryUnlock();
    }

    public void OnNextArrowClicked() => MoveSelection(1);
    public void OnPreviousArrowClicked() => MoveSelection(-1);

    void MoveSelection(int step)
    {
        if (_selectedEntry == null) return;

        int currentPos = _blockedHere.IndexOf(_selectedEntry);
        int nextPos = Mathf.Clamp(currentPos + step, 0, _blockedHere.Count - 1);

        if (nextPos == currentPos) return;

        totemsController.GetView(_selectedEntry.index)?.Hide();

        _selectedEntry = _blockedHere[nextPos];
        ShowSelected();
    }

    void TryUnlock()
    {
        if (_selectedEntry == null) return;

        SplineEntry entry = _selectedEntry;
        var view = totemsController.GetView(entry.index);

        if (_stats.Coins < entry.unlockCost)
        {
            view?.PlayDeniedEffect();
            return;
        }

        view?.PlayUnlockEffect(() =>
        {
            _stats.Coins -= entry.unlockCost;
            SplineRuntimeState.Instance.Unblock(entry.index);
            CloseMenu();
        });
    }

    void OpenMenu()
    {
        _menuOpen = true;
        _player.SetMovementLocked(true);
        Time.timeScale = 0f;

        InteractPromptUI.Instance?.Hide();

        _selectedEntry = _blockedHere[0];
        ShowSelected();
    }

    void ShowSelected()
    {
        if (_selectedEntry == null) return;

        var view = totemsController.GetView(_selectedEntry.index);
        if (view == null) return;

        view.Bind(_selectedEntry, _stats.Coins >= _selectedEntry.unlockCost, TryUnlock);
        view.Show();
        view.SetSelected(true);

        focusDim?.SetFocused(true, _selectedEntry.themeColor);
    }

    void CloseMenu()
    {
        _menuOpen = false;
        _player?.SetMovementLocked(false);
        Time.timeScale = 1f;

        focusDim?.SetFocused(false, Color.white);

        if (_selectedEntry != null)
            totemsController.GetView(_selectedEntry.index)?.Hide();

        _selectedEntry = null;
    }

    List<SplineEntry> ResolveRelevantEntries(int currentSplineIndex)
    {
        var result = new List<SplineEntry>();

        if (splineContainer == null || SplineRuntimeState.Instance?.manifest == null)
            return result;

        KnotLinkCollection links = splineContainer.KnotLinkCollection;
        if (links == null) return result;

        Spline currentSpline = splineContainer.Splines[currentSplineIndex];
        int closestKnot = GetClosestKnotIndex(currentSpline);
        var currentKnotIdx = new SplineKnotIndex(currentSplineIndex, closestKnot);

        IReadOnlyList<SplineKnotIndex> linked = links.GetKnotLinks(currentKnotIdx);
        if (linked == null) return result;

        foreach (var ski in linked)
        {
            if (ski.Spline == currentSplineIndex) continue;

            SplineEntry entry = SplineRuntimeState.Instance.manifest.GetEntry(ski.Spline);
            if (entry != null && !result.Contains(entry))
                result.Add(entry);
        }

        return result;
    }

    int GetClosestKnotIndex(Spline spline)
    {
        Vector3 jPos = transform.position;
        int closest = 0;
        float closestDist = float.MaxValue;

        for (int k = 0; k < spline.Count; k++)
        {
            Vector3 kw = splineContainer.transform.TransformPoint(spline[k].Position);
            float dist = Vector3.Distance(
                new Vector3(jPos.x, 0f, jPos.z),
                new Vector3(kw.x, 0f, kw.z));
            if (dist < closestDist) { closestDist = dist; closest = k; }
        }
        return closest;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _player = other.GetComponent<PlayerController>();
        _stats = other.GetComponent<PlayerStatsAggregator>();

        if (_player != null)
            _relevantEntries = ResolveRelevantEntries(_player.CurrentSplineIndex);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (_menuOpen) CloseMenu();

        InteractPromptUI.Instance?.Hide();

        _player = null;
        _stats = null;
        _relevantEntries.Clear();
        _blockedHere.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, unlockRadius);
        Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, unlockRadius);
    }
}