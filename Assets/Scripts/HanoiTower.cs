using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Tower of Hanoi controller.
/// Click a peg to pick up its top disc, click another peg to drop it there.
/// Larger discs can never be placed on smaller ones.
/// </summary>
public class HanoiTower : MonoBehaviour
{
    private const float UiDiscStackSpacing = 0.7f;
    private const float UiLiftTopPadding = 8f;

    [Header("Setup")]
    [Tooltip("The 3 pegs. Each needs a Collider so it can be clicked.")]
    [SerializeField] private Transform[] pegs = new Transform[3];

    [Tooltip("The 6 discs ordered from LARGEST (index 0) to SMALLEST (index 5).")]
    [SerializeField] private Transform[] discs = new Transform[6];

    [Header("Layout")]
    [Tooltip("Height of one disc (world units). Used to stack them.")]
    [SerializeField] private float discHeight = 0.3f;

    [Tooltip("Offset from the peg's position to where the bottom disc sits.")]
    [SerializeField] private float baseOffset = 0.15f;

    [Tooltip("How high above the pegs discs travel while being moved.")]
    [SerializeField] private float liftHeight = 3f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onInvalidMove;
    public UnityEvent<int> onMoveCountChanged;

    private Stack<Transform>[] stacks;
    private Dictionary<Transform, int> discSize; // higher number = bigger disc

    private Transform heldDisc;
    private int heldFrom = -1;
    private bool busy;
    private int moves;
    private bool won;

    private Camera cam;
    private GraphicRaycaster uiRaycaster;
    private RectTransform uiCanvasRect;
    private bool isUiPuzzle;

    public int Moves => moves;

    private void Start()
    {
        if (!ValidateSetup()) return;

        cam = Camera.main;
        isUiPuzzle = pegs[0] is RectTransform && discs[0] is RectTransform;
        if (isUiPuzzle)
        {
            Canvas canvas = pegs[0].GetComponentInParent<Canvas>();
            uiRaycaster = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
            uiCanvasRect = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
            if (uiRaycaster == null || uiCanvasRect == null)
            {
                Debug.LogError("HanoiTower UI setup needs a GraphicRaycaster and a RectTransform Canvas.", this);
                enabled = false;
                return;
            }
        }
        else if (cam == null)
        {
            Debug.LogError("HanoiTower needs a camera tagged MainCamera to click world-space pegs.", this);
            enabled = false;
            return;
        }

        ResetPuzzle(true);
    }

    private bool ValidateSetup()
    {
        if (pegs == null || pegs.Length != 3 || discs == null || discs.Length == 0)
        {
            Debug.LogError("HanoiTower setup needs exactly 3 pegs and at least 1 disc.", this);
            enabled = false;
            return false;
        }

        for (int i = 0; i < pegs.Length; i++)
        {
            if (pegs[i] == null)
            {
                Debug.LogError($"HanoiTower Pegs element {i} is not assigned in the Inspector.", this);
                enabled = false;
                return false;
            }
        }

        for (int i = 0; i < discs.Length; i++)
        {
            if (discs[i] == null)
            {
                Debug.LogError($"HanoiTower Discs element {i} is not assigned in the Inspector.", this);
                enabled = false;
                return false;
            }
        }

        bool uiSetup = pegs[0] is RectTransform && discs[0] is RectTransform;
        for (int i = 0; i < pegs.Length; i++)
        {
            if ((pegs[i] is RectTransform) != uiSetup)
            {
                Debug.LogError("HanoiTower pegs and discs must all use either UI RectTransforms or world-space Transforms.", this);
                enabled = false;
                return false;
            }
        }

        for (int i = 0; i < discs.Length; i++)
        {
            if ((discs[i] is RectTransform) != uiSetup)
            {
                Debug.LogError("HanoiTower pegs and discs must all use either UI RectTransforms or world-space Transforms.", this);
                enabled = false;
                return false;
            }
        }

        return true;
    }

    private void Update()
    {
        if (busy || won) return;

        if (Input.GetMouseButtonDown(0))
        {
            int peg = isUiPuzzle ? GetUiPegUnderMouse() : GetWorldPegUnderMouse();
            if (peg >= 0) OnPegClicked(peg);
        }
    }

    // ---------- Setup ----------

    [ContextMenu("Reset Puzzle")]
    public void ResetPuzzle() => ResetPuzzle(true);

    public void ResetPuzzle(bool instant)
    {
        StopAllCoroutines();
        busy = false;
        won = false;
        moves = 0;
        heldDisc = null;
        heldFrom = -1;
        onMoveCountChanged?.Invoke(moves);

        stacks = new Stack<Transform>[pegs.Length];
        for (int i = 0; i < stacks.Length; i++) stacks[i] = new Stack<Transform>();

        discSize = new Dictionary<Transform, int>();
        for (int i = 0; i < discs.Length; i++)
        {
            // index 0 is the largest, so size = (count - i)
            discSize[discs[i]] = discs.Length - i;

            // Push largest first so the smallest ends up on top
            discs[i].position = GetSlotPosition(0, stacks[0].Count, discs[i]);
            stacks[0].Push(discs[i]);
        }
    }

    // ---------- Input ----------

    private int GetUiPegUnderMouse()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("HanoiTower UI clicks need an active EventSystem in the scene.", this);
            enabled = false;
            return -1;
        }

        var pointer = new PointerEventData(eventSystem) { position = Input.mousePosition };
        var hits = new List<RaycastResult>();
        uiRaycaster.Raycast(pointer, hits);

        foreach (RaycastResult hit in hits)
        {
            Transform hitTransform = hit.gameObject.transform;
            for (int i = 0; i < pegs.Length; i++)
            {
                if (hitTransform == pegs[i] || hitTransform.IsChildOf(pegs[i]))
                    return i;
            }
        }

        return -1;
    }

    private int GetWorldPegUnderMouse()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return -1;

        for (int i = 0; i < pegs.Length; i++)
        {
            if (hit.transform == pegs[i] || hit.transform.IsChildOf(pegs[i]))
                return i;
        }
        return -1;
    }

    private void OnPegClicked(int pegIndex)
    {
        if (heldDisc == null)
        {
            // Pick up the top disc
            if (stacks[pegIndex].Count == 0) return;

            heldDisc = stacks[pegIndex].Pop();
            heldFrom = pegIndex;
            StartCoroutine(MoveTo(heldDisc, GetLiftPosition(pegIndex, heldDisc)));
        }
        else
        {
            // Try to drop the held disc
            if (pegIndex == heldFrom || CanPlace(heldDisc, pegIndex))
            {
                StartCoroutine(DropDisc(pegIndex));
            }
            else
            {
                onInvalidMove?.Invoke();
                StartCoroutine(ReturnDisc());
            }
        }
    }

    // ---------- Rules ----------

    private bool CanPlace(Transform disc, int pegIndex)
    {
        if (stacks[pegIndex].Count == 0) return true;
        return discSize[disc] < discSize[stacks[pegIndex].Peek()];
    }

    private bool CheckWin()
    {
        // Win when all discs are on the last peg
        return stacks[pegs.Length - 1].Count == discs.Length;
    }

    // ---------- Movement ----------

    private IEnumerator DropDisc(int pegIndex)
    {
        busy = true;
        Transform disc = heldDisc;

        // Slide over the target peg, then lower into place
        yield return MoveTo(disc, GetLiftPosition(pegIndex, disc), true);
        yield return MoveTo(disc, GetSlotPosition(pegIndex, stacks[pegIndex].Count, disc), true);

        stacks[pegIndex].Push(disc);

        if (pegIndex != heldFrom)
        {
            moves++;
            onMoveCountChanged?.Invoke(moves);
        }

        heldDisc = null;
        heldFrom = -1;
        busy = false;

        if (CheckWin())
        {
            won = true;
            Debug.Log($"Tower of Hanoi solved in {moves} moves! (minimum: {(1 << discs.Length) - 1})");
            onWin?.Invoke();
        }
    }

    private IEnumerator ReturnDisc()
    {
        busy = true;
        Transform disc = heldDisc;
        int from = heldFrom;

        yield return MoveTo(disc, GetLiftPosition(from, disc), true);
        yield return MoveTo(disc, GetSlotPosition(from, stacks[from].Count, disc), true);

        stacks[from].Push(disc);
        heldDisc = null;
        heldFrom = -1;
        busy = false;
    }

    private IEnumerator MoveTo(Transform t, Vector3 target, bool lockBusy = false)
    {
        if (!lockBusy) busy = true;

        while ((t.position - target).sqrMagnitude > 0.0001f)
        {
            t.position = Vector3.MoveTowards(t.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        t.position = target;

        if (!lockBusy) busy = false;
    }

    // ---------- Positions ----------

    private Vector3 GetSlotPosition(int pegIndex, int level, Transform disc)
    {
        if (isUiPuzzle)
        {
            RectTransform pegRect = (RectTransform)pegs[pegIndex];
            RectTransform discRect = (RectTransform)disc;
            Transform discParent = discRect.parent;
            Vector3 localBottom = GetUiPegBottomInParent(pegRect, discParent);
            float discHeightInParent = GetUiRenderedDiscHeightInParent(discRect, discParent);
            float discsBelowHeight = GetUiStackHeightInParent(stacks[pegIndex], discParent);

            return discParent.TransformPoint(
                localBottom + Vector3.up * (discsBelowHeight * UiDiscStackSpacing + discHeightInParent * 0.5f));
        }

        return pegs[pegIndex].position + Vector3.up * (baseOffset + level * discHeight);
    }

    private Vector3 GetLiftPosition(int pegIndex, Transform disc)
    {
        if (isUiPuzzle)
        {
            RectTransform pegRect = (RectTransform)pegs[pegIndex];
            RectTransform discRect = (RectTransform)disc;
            Transform discParent = discRect.parent;
            Vector3 localBottom = GetUiPegBottomInParent(pegRect, discParent);
            float discHeightInParent = GetUiRenderedDiscHeightInParent(discRect, discParent);
            float stackHeight = GetUiStackHeightInParent(stacks[pegIndex], discParent);
            float topDiscHeight = stacks[pegIndex].Count > 0
                ? GetUiRenderedDiscHeightInParent((RectTransform)stacks[pegIndex].Peek(), discParent)
                : 0f;
            float stackVisualTop = stacks[pegIndex].Count > 0
                ? (stackHeight - topDiscHeight) * UiDiscStackSpacing + topDiscHeight
                : 0f;
            float desiredY = localBottom.y + stackVisualTop + discHeightInParent * 0.75f;
            float canvasTop = GetUiRectTopInParent(uiCanvasRect, discParent);
            float maxCenterY = canvasTop - discHeightInParent * 0.5f - UiLiftTopPadding;

            return discParent.TransformPoint(new Vector3(
                localBottom.x,
                Mathf.Min(desiredY, maxCenterY),
                localBottom.z));
        }

        return pegs[pegIndex].position + Vector3.up * liftHeight;
    }

    private static Vector3 GetUiPegBottomInParent(RectTransform peg, Transform parent)
    {
        Vector3 pegBottom = peg.TransformPoint(new Vector3(0f, peg.rect.yMin, 0f));
        return parent.InverseTransformPoint(pegBottom);
    }

    private static float GetUiRenderedDiscHeightInParent(RectTransform disc, Transform parent)
    {
        float renderedHeight = disc.rect.height;
        Image image = disc.GetComponent<Image>();
        if (image != null && image.preserveAspect && image.sprite != null)
        {
            float aspect = image.sprite.rect.width / image.sprite.rect.height;
            if (aspect > 0f)
                renderedHeight = Mathf.Min(renderedHeight, disc.rect.width / aspect);
        }

        Vector3 discHeight = parent.InverseTransformVector(
            disc.TransformVector(Vector3.up * renderedHeight));
        return Mathf.Abs(discHeight.y);
    }

    private static float GetUiStackHeightInParent(Stack<Transform> stack, Transform parent)
    {
        float height = 0f;
        foreach (Transform stackedDisc in stack)
            height += GetUiRenderedDiscHeightInParent((RectTransform)stackedDisc, parent);
        return height;
    }

    private static float GetUiRectTopInParent(RectTransform rect, Transform parent)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return Mathf.Max(
            parent.InverseTransformPoint(corners[1]).y,
            parent.InverseTransformPoint(corners[2]).y);
    }
}