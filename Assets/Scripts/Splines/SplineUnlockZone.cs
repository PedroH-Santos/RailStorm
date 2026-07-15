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

    readonly Dictionary<int, string> _entryDirections = new();

    int _selectedIndex = -1;

    // Debounce do analógico do gamepad, pra não disparar troca em todo frame
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

        // Prompt "Aperte E" — só aparece quando há bloqueio e o menu ainda não abriu
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

        // Confirmar — teclado
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            TryUnlock(_selectedIndex);

        // Confirmar — gamepad (botão X do PlayStation = buttonWest no Input System)
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
            TryUnlock(_selectedIndex);
    }

    void MoveSelection(int dir)
    {
        if (_blockedHere.Count <= 1) return;

        // Esconde a UI do totem atual antes de trocar
        var currentEntry = _blockedHere[_selectedIndex];
        totemsController.GetView(currentEntry.index)?.Hide();

        _selectedIndex = (_selectedIndex + dir + _blockedHere.Count) % _blockedHere.Count;

        ShowSelected();
    }

    void TryUnlock(int menuIndex)
    {
        if (menuIndex < 0 || menuIndex >= _blockedHere.Count) return;

        SplineEntry entry = _blockedHere[menuIndex];
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

            _blockedHere = SplineRuntimeState.Instance
                .GetBlockedEntriesFrom(_relevantEntries)
                .ToList();

            if (_blockedHere.Count == 0)
                CloseMenu();
            else
            {
                _selectedIndex = 0;
                ShowSelected();
            }
        });
    }

    void OpenMenu()
    {
        _menuOpen = true;
        _player.SetMovementLocked(true);
        Time.timeScale = 0f;

        InteractPromptUI.Instance?.Hide();

        _selectedIndex = 0;
        ShowSelected();
    }

    void ShowSelected()
    {
        if (_blockedHere.Count == 0) return;

        var entry = _blockedHere[_selectedIndex];
        var view = totemsController.GetView(entry.index);

        view.Bind(entry, _stats.Coins >= entry.unlockCost, () => TryUnlock(_selectedIndex));
        view.Show();
        view.SetSelected(true);

        focusDim?.SetFocused(true, entry.themeColor);
    }

    void CloseMenu()
    {
        _menuOpen = false;
        _player?.SetMovementLocked(false);
        Time.timeScale = 1f;

        focusDim?.SetFocused(false, Color.white);

        if (_selectedIndex >= 0 && _selectedIndex < _blockedHere.Count)
        {
            var entry = _blockedHere[_selectedIndex];
            totemsController.GetView(entry.index)?.Hide();
        }

        _selectedIndex = -1;
    }

    string GetDirectionArrow(SplineEntry entry)
    {
        if (splineContainer == null) return "→";

        Spline spline = splineContainer.Splines[entry.index];
        if (spline == null || spline.Count == 0) return "→";

        int closestKnot = GetClosestKnotIndex(spline);
        float knotT = SplineUtility.GetNormalizedInterpolation(spline, closestKnot, PathIndexUnit.Knot);

        Vector3 junctionPos = transform.position;
        Vector3 origin = splineContainer.transform.TransformPoint(spline.EvaluatePosition(knotT));

        float sampleStep = 0.08f;

        float tFwd = spline.Closed
            ? Mathf.Repeat(knotT + sampleStep, 1f)
            : Mathf.Clamp01(knotT + sampleStep);
        float tBwd = spline.Closed
            ? Mathf.Repeat(knotT - sampleStep + 1f, 1f)
            : Mathf.Clamp01(knotT - sampleStep);

        Vector3 pFwd = splineContainer.transform.TransformPoint(spline.EvaluatePosition(tFwd));
        Vector3 pBwd = splineContainer.transform.TransformPoint(spline.EvaluatePosition(tBwd));

        float distFwd = Vector3.Distance(new Vector3(pFwd.x, 0f, pFwd.z),
                                         new Vector3(junctionPos.x, 0f, junctionPos.z));
        float distBwd = Vector3.Distance(new Vector3(pBwd.x, 0f, pBwd.z),
                                         new Vector3(junctionPos.x, 0f, junctionPos.z));

        Vector3 chosenPoint = distFwd >= distBwd ? pFwd : pBwd;
        Vector3 dir = chosenPoint - origin;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return "→";

        dir.Normalize();

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        int sector = Mathf.RoundToInt(angle / 45f) % 8;
        return sector switch
        {
            0 => "↑",
            1 => "↗",
            2 => "→",
            3 => "↘",
            4 => "↓",
            5 => "↙",
            6 => "←",
            7 => "↖",
            _ => "→"
        };
    }

    List<SplineEntry> ResolveRelevantEntries(int currentSplineIndex)
    {
        var result = new List<SplineEntry>();
        _entryDirections.Clear();

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
            {
                result.Add(entry);
                _entryDirections[entry.index] = GetDirectionArrow(entry);
            }
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
        _entryDirections.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, unlockRadius);
        Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, unlockRadius);
    }
}