using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System.Collections;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;
    public FlyingObjectManager flyingObjectManager;

    public event Action OnRewardedAdCompleted;

    private bool isLoading;
    private bool isReady;
    private bool isShowing;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;
        if (flyingObjectManager == null)
            flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load rewarded ad before Unity Ads was initialized.", this);
            return;
        }

        if (isLoading || isReady || isShowing)
            return;

        isLoading = true;
        Debug.Log("Loading rewarded ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != _adUnitId)
            return;

        isLoading = false;
        isReady = true;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = true;
    }

    public void ShowAd()
    {
        if (!isReady || isShowing || !Advertisement.isInitialized)
        {
            Debug.LogWarning("Rewarded ad is not ready to show.", this);
            return;
        }

        isReady = false;
        isShowing = true;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        if (AdManager.Instance != null &&
            AdManager.Instance.bannerAd != null &&
            AdManager.Instance.bannerAd.isBannerVisable)
        {
            AdManager.Instance.bannerAd.HideBannerAd();
        }

        Advertisement.Show(_adUnitId, this);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if (placementId != _adUnitId)
            return;

        isLoading = false;
        Debug.LogWarning($"Error loading rewarded ad unit {placementId}: {error} - {message}", this);
        StartCoroutine(WaitAndLoadAd(10f));
    }

    private IEnumerator WaitAndLoadAd(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        if (placementId != _adUnitId)
            return;

        isShowing = false;
        Time.timeScale = 1f;
        Debug.LogError($"Error showing rewarded ad unit {placementId}: {error} - {message}", this);
        ShowBannerAfterAd();
        StartCoroutine(WaitAndLoadAd(10f));
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        if (placementId != _adUnitId)
            return;

        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on the rewarded ad: " + placementId, this);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId != _adUnitId)
            return;

        isShowing = false;
        Time.timeScale = 1f;
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Unity Ads rewarded ad completed.", this);
            if (flyingObjectManager != null)
                flyingObjectManager.DestroyAllFlyingObjects();
            OnRewardedAdCompleted?.Invoke();
        }

        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;
        ShowBannerAfterAd();
        StartCoroutine(WaitAndLoadAd(15f));
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = isReady;
    }

    private static void ShowBannerAfterAd()
    {
        if (AdManager.Instance != null && AdManager.Instance.bannerAd != null)
            AdManager.Instance.bannerAd.ShowBannerAd();
    }
}
