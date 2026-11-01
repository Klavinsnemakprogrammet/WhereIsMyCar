using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HanoiTower : MonoBehaviour
{
    public const int MinimumDiscCount = 3;

    private const int MaximumDiscCount = 6;

    [Header("Puzzle Objects")]
    [SerializeField] private Transform[] pegs = new Transform[3];
    [Tooltip("Assign discs from largest to smallest.")]
    [SerializeField] private Transform[] discs = new Transform[MaximumDiscCount];
    [Header("Disc Layout")]
    [SerializeField, Min(1f)] private float largestDiscWidth = 420f;
    [SerializeField, Min(1f)] private float smallestDiscWidth = 210f;
    [SerializeField, Min(0f)] private float uiBaseOffset;
    [SerializeField, Min(0f)] private float uiLiftHeight = 24f;
    [SerializeField, Min(1f)] private float uiMoveSpeed = 1000f;

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onInvalidMove;
    public UnityEvent<int> onMoveCountChanged;

    private readonly Stack<Transform>[] stacks = new Stack<Transform>[3];
    private readonly Dictionary<Transform, int> discRanks = new Dictionary<Transform, int>();
    private readonly List<UnityAction> installedListeners = new List<UnityAction>();
    private readonly List<Button> installedButtons = new List<Button>();

    private Transform selectedDisc;
    private int selectedFrom = -1;
    private int activeDiscCount;
    private int moves;
    private int rewardedMoveCredit;
    private bool busy;
    private bool won;
    private RectTransform discParent;
    private RectTransform canvasRect;
    private float stackBaseY;
    private bool stackBaseInitialized;
    private bool initialized;

    public int Moves => Mathf.Max(0, moves - rewardedMoveCredit);
    public int DiscCount => activeDiscCount;
    public int DiscCapacity => discs != null ? discs.Length : 0;

    private void OnValidate()
    {
        if (!Application.isPlaying || !stackBaseInitialized || discParent == null || stacks[0] == null)
            return;

        for (int pegIndex = 0; pegIndex < stacks.Length; pegIndex++)
        {
            if (stacks[pegIndex] == null)
                continue;

            Transform[] stackDiscs = stacks[pegIndex].ToArray();
            float heightBelowDisc = 0f;
            for (int i = stackDiscs.Length - 1; i >= 0; i--)
            {
                Transform disc = stackDiscs[i];
                disc.localPosition = GetSlotPosition(pegIndex, heightBelowDisc, disc);
                heightBelowDisc += GetDiscHeight(disc);
            }
        }

        if (selectedDisc != null && selectedFrom >= 0)
            selectedDisc.localPosition = GetAbovePegsPosition(selectedDisc, selectedFrom);
    }

    private void Start()
    {
        if (!ValidateSetup())
            return;

        if (EventSystem.current == null)
        {
            Debug.LogError("HanoiTower requires an active EventSystem in the scene.", this);
            enabled = false;
            return;
        }

        Canvas canvas = pegs[0].GetComponentInParent<Canvas>();
        if (canvas == null || canvas.rootCanvas == null)
        {
            Debug.LogError("HanoiTower pegs must be children of a Canvas.", this);
            enabled = false;
            return;
        }

        canvasRect = canvas.rootCanvas.transform as RectTransform;
        if (canvasRect == null)
        {
            Debug.LogError("HanoiTower requires a Canvas with a RectTransform.", this);
            enabled = false;
            return;
        }

        stackBaseY = GetAuthoredStackBaseY();
        stackBaseInitialized = true;
        SetupObjectButtons();

        activeDiscCount = activeDiscCount > 0
            ? Mathf.Clamp(activeDiscCount, MinimumDiscCount, discs.Length)
            : discs.Length;
        ResetPuzzle();
        initialized = true;
    }

    private bool ValidateSetup()
    {
        if (pegs == null || pegs.Length != 3 || discs == null || discs.Length < MinimumDiscCount ||
            discs.Length > MaximumDiscCount)
        {
            Debug.LogError("HanoiTower needs exactly 3 pegs and between 3 and 6 discs.", this);
            enabled = false;
            return false;
        }

        discParent = discs[0] != null ? discs[0].parent as RectTransform : null;
        if (discParent == null)
        {
            Debug.LogError("HanoiTower discs must be UI Images under a shared RectTransform parent.", this);
            enabled = false;
            return false;
        }

        for (int i = 0; i < pegs.Length; i++)
        {
            if (pegs[i] == null || !(pegs[i] is RectTransform) || pegs[i].GetComponent<Image>() == null)
            {
                Debug.LogError($"HanoiTower peg {i} must be assigned as a UI Image.", this);
                enabled = false;
                return false;
            }
        }

        for (int i = 0; i < discs.Length; i++)
        {
            Image image = discs[i] != null ? discs[i].GetComponent<Image>() : null;
            if (discs[i] == null || !(discs[i] is RectTransform) ||
                discs[i].parent != discParent || image == null || image.sprite == null ||
                image.sprite.rect.width <= 0f || image.sprite.rect.height <= 0f)
            {
                Debug.LogError($"HanoiTower disc {i} must be a UI Image under the same parent as the other discs.", this);
                enabled = false;
                return false;
            }

            if (discRanks.ContainsKey(discs[i]))
            {
                Debug.LogError("HanoiTower disc references must be unique.", this);
                enabled = false;
                return false;
            }

            discRanks.Add(discs[i], i);
        }

        return true;
    }

    private void SetupObjectButtons()
    {
        for (int i = 0; i < pegs.Length; i++)
        {
            int pegIndex = i;
            Button button = GetOrAddButton(pegs[i]);
            UnityAction listener = () => OnPegClicked(pegIndex);
            button.onClick.AddListener(listener);
            installedButtons.Add(button);
            installedListeners.Add(listener);
        }

        for (int i = 0; i < discs.Length; i++)
        {
            int discIndex = i;
            Button button = GetOrAddButton(discs[i]);
            UnityAction listener = () => OnDiscClicked(discIndex);
            button.onClick.AddListener(listener);
            installedButtons.Add(button);
            installedListeners.Add(listener);
        }
    }

    private static Button GetOrAddButton(Transform target)
    {
        Image image = target.GetComponent<Image>();
        image.raycastTarget = true;

        Button button = target.GetComponent<Button>();
        if (button == null)
            button = target.gameObject.AddComponent<Button>();

        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        return button;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < installedButtons.Count; i++)
        {
            if (installedButtons[i] != null)
                installedButtons[i].onClick.RemoveListener(installedListeners[i]);
        }
    }

    public void SetDiscCount(int count)
    {
        count = Mathf.Max(count, MinimumDiscCount);

        if (!initialized)
        {
            activeDiscCount = count;
            return;
        }

        int newCount = Mathf.Clamp(count, MinimumDiscCount, discs.Length);
        if (newCount == activeDiscCount)
            return;

        activeDiscCount = newCount;
        ResetPuzzle();
    }

    public void ApplyRewardedMoveDiscount(int count)
    {
        if (count <= 0 || won)
            return;

        rewardedMoveCredit += count;
        onMoveCountChanged?.Invoke(Moves);
    }

    [ContextMenu("Reset Puzzle")]
    public void ResetPuzzle()
    {
        if (discs == null || discs.Length == 0 || discParent == null)
            return;

        StopAllCoroutines();
        busy = false;
        won = false;
        moves = 0;
        rewardedMoveCredit = 0;
        selectedDisc = null;
        selectedFrom = -1;
        onMoveCountChanged?.Invoke(Moves);

        for (int i = 0; i < stacks.Length; i++)
            stacks[i] = new Stack<Transform>();

        float widthStep = activeDiscCount > 1
            ? (largestDiscWidth - smallestDiscWidth) / (activeDiscCount - 1)
            : 0f;

        for (int i = 0; i < discs.Length; i++)
        {
            RectTransform rect = (RectTransform)discs[i];
            Image image = discs[i].GetComponent<Image>();
            float width = largestDiscWidth - widthStep * i;
            float aspect = image.sprite.rect.width / image.sprite.rect.height;
            image.preserveAspect = true;
            rect.sizeDelta = new Vector2(width, width / aspect);
            discs[i].gameObject.SetActive(i < activeDiscCount);

            if (i < activeDiscCount)
            {
                rect.localPosition = GetSlotPosition(0, GetStackHeight(stacks[0]), discs[i]);
                stacks[0].Push(discs[i]);
            }
        }

    }

    public void ResetPuzzle(bool instant)
    {
        ResetPuzzle();
    }

    private void OnDiscClicked(int discIndex)
    {
        if (busy || won || discIndex >= activeDiscCount)
            return;

        Transform disc = discs[discIndex];
        int pegIndex = FindPegWithTopDisc(disc);
        if (pegIndex < 0)
            return;

        if (selectedDisc == disc)
        {
            StartCoroutine(DeselectDisc());
        }
        else if (selectedDisc == null)
        {
            selectedDisc = disc;
            selectedFrom = pegIndex;
            StartCoroutine(MoveSelectedToLift());
        }
        else
        {
            StartCoroutine(SwitchSelectedDisc(disc, pegIndex));
        }
    }

    private void OnPegClicked(int pegIndex)
    {
        if (busy || won || selectedDisc == null)
            return;

        if (pegIndex == selectedFrom)
        {
            StartCoroutine(DeselectDisc());
            return;
        }

        if (stacks[pegIndex].Count > 0 &&
            discRanks[selectedDisc] <= discRanks[stacks[pegIndex].Peek()])
        {
            onInvalidMove?.Invoke();
            StartCoroutine(DeselectDisc());
            return;
        }

        StartCoroutine(PlaceSelectedDisc(pegIndex));
    }

    private int FindPegWithTopDisc(Transform disc)
    {
        for (int i = 0; i < stacks.Length; i++)
        {
            if (stacks[i].Count > 0 && stacks[i].Peek() == disc)
                return i;
        }

        return -1;
    }

    private IEnumerator MoveSelectedToLift()
    {
        busy = true;
        yield return MoveTo(selectedDisc, GetAbovePegsPosition(selectedDisc, selectedFrom));
        busy = false;
    }

    private IEnumerator DeselectDisc()
    {
        busy = true;
        Transform disc = selectedDisc;
        int source = selectedFrom;
        float heightBelowDisc = GetStackHeight(stacks[source]) - GetDiscHeight(disc);
        yield return MoveTo(disc, GetSlotPosition(source, heightBelowDisc, disc));
        selectedDisc = null;
        selectedFrom = -1;
        busy = false;
    }

    private IEnumerator SwitchSelectedDisc(Transform nextDisc, int nextPeg)
    {
        busy = true;
        Transform previousDisc = selectedDisc;
        int previousPeg = selectedFrom;
        float heightBelowDisc = GetStackHeight(stacks[previousPeg]) - GetDiscHeight(previousDisc);
        yield return MoveTo(previousDisc, GetSlotPosition(previousPeg, heightBelowDisc, previousDisc));

        selectedDisc = nextDisc;
        selectedFrom = nextPeg;
        yield return MoveTo(selectedDisc, GetAbovePegsPosition(selectedDisc, selectedFrom));
        busy = false;
    }

    private IEnumerator PlaceSelectedDisc(int targetPeg)
    {
        busy = true;
        Transform disc = selectedDisc;
        int sourcePeg = selectedFrom;

        stacks[sourcePeg].Pop();
        float heightBelowDisc = GetStackHeight(stacks[targetPeg]);
        stacks[targetPeg].Push(disc);
        yield return MoveTo(disc, GetAbovePegsPosition(disc, targetPeg));
        yield return MoveTo(disc, GetSlotPosition(targetPeg, heightBelowDisc, disc));

        selectedDisc = null;
        selectedFrom = -1;
        moves++;
        onMoveCountChanged?.Invoke(Moves);
        busy = false;

        if (stacks[pegs.Length - 1].Count == activeDiscCount)
        {
            won = true;
            Debug.Log($"Tower of Hanoi solved in {moves} moves (minimum: {Mathf.Pow(2, activeDiscCount) - 1}).", this);
            onWin?.Invoke();
        }
    }

    private IEnumerator MoveTo(Transform target, Vector3 destination)
    {
        RectTransform rect = (RectTransform)target;
        Vector3 start = rect.localPosition;
        float distance = Vector3.Distance(start, destination);
        if (distance <= 0.01f)
        {
            rect.localPosition = destination;
            yield break;
        }

        float duration = distance / Mathf.Max(1f, uiMoveSpeed);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            rect.localPosition = Vector3.LerpUnclamped(start, destination, t);
            yield return null;
        }

        rect.localPosition = destination;
    }

    private float GetDiscHeight(Transform disc)
    {
        return ((RectTransform)disc).rect.height;
    }

    private float GetAuthoredStackBaseY()
    {
        RectTransform rect = (RectTransform)discs[0];
        Image image = discs[0].GetComponent<Image>();
        float aspect = image.sprite.rect.width / image.sprite.rect.height;
        float renderedHeight = image.preserveAspect
            ? Mathf.Min(rect.rect.height, rect.rect.width / aspect)
            : rect.rect.height;
        float visualBottomFromPivot = rect.rect.height * (0.5f - rect.pivot.y) - renderedHeight * 0.5f;
        return rect.localPosition.y + visualBottomFromPivot;
    }

    private static float GetStackHeight(Stack<Transform> stack)
    {
        float height = 0f;
        foreach (Transform disc in stack)
            height += ((RectTransform)disc).rect.height;
        return height;
    }

    private Vector3 GetSlotPosition(int pegIndex, float heightBelowDisc, Transform disc)
    {
        Vector3[] corners = new Vector3[4];
        ((RectTransform)pegs[pegIndex]).GetWorldCorners(corners);
        Vector3 pegCenter = discParent.InverseTransformPoint((corners[0] + corners[2]) * 0.5f);
        pegCenter.y = stackBaseY + uiBaseOffset + heightBelowDisc + GetDiscHeight(disc) * 0.5f;
        pegCenter.z = 0f;
        return pegCenter;
    }

    private Vector3 GetAbovePegsPosition(Transform disc, int pegIndex)
    {
        Vector3 pegCenter = GetPegCenterInDiscParent(pegIndex);
        float highestPegTop = float.NegativeInfinity;
        for (int i = 0; i < pegs.Length; i++)
            highestPegTop = Mathf.Max(highestPegTop, GetPegTopInDiscParent(i));

        float discHalfHeight = GetDiscHeight(disc) * 0.5f;
        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);
        float canvasTop = Mathf.Max(
            discParent.InverseTransformPoint(canvasCorners[1]).y,
            discParent.InverseTransformPoint(canvasCorners[2]).y);

        pegCenter.y = Mathf.Min(
            highestPegTop + uiLiftHeight + discHalfHeight,
            canvasTop - discHalfHeight - 8f);
        return pegCenter;
    }

    private float GetPegTopInDiscParent(int pegIndex)
    {
        Vector3[] corners = new Vector3[4];
        ((RectTransform)pegs[pegIndex]).GetWorldCorners(corners);
        return Mathf.Max(
            discParent.InverseTransformPoint(corners[1]).y,
            discParent.InverseTransformPoint(corners[2]).y);
    }

    private Vector3 GetPegCenterInDiscParent(int pegIndex)
    {
        Vector3[] corners = new Vector3[4];
        ((RectTransform)pegs[pegIndex]).GetWorldCorners(corners);
        Vector3 center = (corners[0] + corners[2]) * 0.5f;
        return discParent.InverseTransformPoint(center);
    }

}
