using TMPro;
using UnityEngine;

public class HanoiMoveCounter : MonoBehaviour
{
    [SerializeField] private HanoiTower hanoiTower;

    private TMP_Text counterText;

    private void Awake()
    {
        counterText = GetComponent<TMP_Text>();
        if (counterText == null)
        {
            Debug.LogError("HanoiMoveCounter must be attached to a TextMeshPro text object.", this);
            enabled = false;
            return;
        }

        hanoiTower ??= FindFirstObjectByType<HanoiTower>();

        if (hanoiTower == null)
        {
            Debug.LogError("HanoiMoveCounter could not find a HanoiTower in the scene.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (hanoiTower == null || counterText == null)
            return;

        if (hanoiTower.onMoveCountChanged == null)
        {
            Debug.LogError("HanoiMoveCounter's HanoiTower has no move-count event.", hanoiTower);
            enabled = false;
            return;
        }

        hanoiTower.onMoveCountChanged.AddListener(UpdateCounter);
        UpdateCounter(hanoiTower.Moves);
    }

    private void OnDisable()
    {
        if (hanoiTower != null && hanoiTower.onMoveCountChanged != null)
            hanoiTower.onMoveCountChanged.RemoveListener(UpdateCounter);
    }

    private void UpdateCounter(int moveCount)
    {
        counterText.text = $"Moves: {moveCount}";
    }
}
