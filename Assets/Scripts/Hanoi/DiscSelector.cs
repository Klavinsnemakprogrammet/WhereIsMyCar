using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiscSelector : MonoBehaviour
{
    [SerializeField] private HanoiTower hanoiTower;
    [SerializeField, Range(HanoiTower.MinimumDiscCount, 6)] private int startingDiscCount = 6;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private TMP_Text countText;

    private int selectedDiscCount;
    private TMP_Text moveCountText;

    private void Awake()
    {
        hanoiTower ??= GetComponent<HanoiTower>();
        if (hanoiTower == null)
        {
            Debug.LogError("DiscSelector needs a HanoiTower reference.", this);
            enabled = false;
            return;
        }

        if (decreaseButton == null && increaseButton == null && countText == null)
        {
            Debug.LogWarning("DiscSelector has no UI components assigned; add and assign the count buttons and text.", this);
            enabled = false;
            return;
        }

        if (decreaseButton == null || increaseButton == null || countText == null)
        {
            Debug.LogError("DiscSelector needs both count buttons and a count Text assigned.", this);
            enabled = false;
            return;
        }

        if (hanoiTower.DiscCapacity < HanoiTower.MinimumDiscCount)
        {
            Debug.LogError("DiscSelector's HanoiTower must have at least three discs assigned.", this);
            enabled = false;
            return;
        }

        selectedDiscCount = Mathf.Clamp(startingDiscCount, HanoiTower.MinimumDiscCount, hanoiTower.DiscCapacity);
        hanoiTower.SetDiscCount(selectedDiscCount);
        moveCountText = FindMoveCountText();
        if (moveCountText != null)
        {
            hanoiTower.onMoveCountChanged.AddListener(UpdateMoveCount);
            UpdateMoveCount(hanoiTower.Moves);
        }
        else
        {
            Debug.LogWarning("DiscSelector could not find a TMP text GameObject named 'Moves'.", this);
        }

        decreaseButton.onClick.AddListener(DecreaseDiscCount);
        increaseButton.onClick.AddListener(IncreaseDiscCount);
        RefreshDisplay();
    }

    private void OnDestroy()
    {
        if (hanoiTower != null && hanoiTower.onMoveCountChanged != null)
            hanoiTower.onMoveCountChanged.RemoveListener(UpdateMoveCount);
        if (decreaseButton != null)
            decreaseButton.onClick.RemoveListener(DecreaseDiscCount);
        if (increaseButton != null)
            increaseButton.onClick.RemoveListener(IncreaseDiscCount);
    }

    private TMP_Text FindMoveCountText()
    {
        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.scene == gameObject.scene && text.gameObject.name == "Moves")
                return text;
        }

        return null;
    }

    private void UpdateMoveCount(int moveCount)
    {
        moveCountText.text = $"Moves: {moveCount}";
    }

    private void IncreaseDiscCount()
    {
        SetDiscCount(selectedDiscCount + 1);
    }

    private void DecreaseDiscCount()
    {
        SetDiscCount(selectedDiscCount - 1);
    }

    private void SetDiscCount(int count)
    {
        selectedDiscCount = Mathf.Clamp(count, HanoiTower.MinimumDiscCount, hanoiTower.DiscCapacity);
        hanoiTower.SetDiscCount(selectedDiscCount);
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        countText.text = $"{selectedDiscCount} discs";
        decreaseButton.interactable = selectedDiscCount > HanoiTower.MinimumDiscCount;
        increaseButton.interactable = selectedDiscCount < hanoiTower.DiscCapacity;
    }
}
