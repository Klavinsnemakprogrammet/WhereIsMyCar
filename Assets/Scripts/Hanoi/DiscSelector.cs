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
        decreaseButton.onClick.AddListener(DecreaseDiscCount);
        increaseButton.onClick.AddListener(IncreaseDiscCount);
        RefreshDisplay();
    }

    private void OnDestroy()
    {
        if (decreaseButton != null)
            decreaseButton.onClick.RemoveListener(DecreaseDiscCount);
        if (increaseButton != null)
            increaseButton.onClick.RemoveListener(IncreaseDiscCount);
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
