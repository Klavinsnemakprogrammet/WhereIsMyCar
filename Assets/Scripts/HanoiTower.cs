using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tower of Hanoi controller.
/// Click a peg to pick up its top disc, click another peg to drop it there.
/// Larger discs can never be placed on smaller ones.
/// </summary>
public class HanoiTower : MonoBehaviour
{
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

    public int Moves => moves;

    private void Start()
    {
        cam = Camera.main;
        ResetPuzzle(true);
    }

    private void Update()
    {
        if (busy || won) return;

        if (Input.GetMouseButtonDown(0))
        {
            int peg = GetPegUnderMouse();
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
            discs[i].position = GetSlotPosition(0, stacks[0].Count);
            stacks[0].Push(discs[i]);
        }
    }

    // ---------- Input ----------

    private int GetPegUnderMouse()
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
            StartCoroutine(MoveTo(heldDisc, GetLiftPosition(pegIndex)));
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
        yield return MoveTo(disc, GetLiftPosition(pegIndex), true);
        yield return MoveTo(disc, GetSlotPosition(pegIndex, stacks[pegIndex].Count), true);

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

        yield return MoveTo(disc, GetLiftPosition(from), true);
        yield return MoveTo(disc, GetSlotPosition(from, stacks[from].Count), true);

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

    private Vector3 GetSlotPosition(int pegIndex, int level)
    {
        return pegs[pegIndex].position + Vector3.up * (baseOffset + level * discHeight);
    }

    private Vector3 GetLiftPosition(int pegIndex)
    {
        return pegs[pegIndex].position + Vector3.up * liftHeight;
    }
}