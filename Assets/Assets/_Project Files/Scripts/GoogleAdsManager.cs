using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoogleAdsManager : MonoBehaviour
{
    public static GoogleAdsManager Instance;
    [SerializeField]
    string bannerAdId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";
    [SerializeField]
    string interstitialAdId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";
    private BannerView bannerView;

    private InterstitialAd interstitialAd;
    //private int gamePlayCount = 0; 
    private const string GamePlayKey = "GamePlayCount";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MobileAds.Initialize(initStatus => { });
            RequestBanner();
            LoadInterstitialAd();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void RequestBanner()
    {
        //string bannerAdID = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx"; // Replace with your real Ad Unit ID

/*#if UNITY_ANDROID
        bannerAdId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IOS
        adUnitId = "your_ios_banner_ad_unit_id";
#else
        adUnitId = "unexpected_platform";
#endif*/
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        bannerView = new BannerView(bannerAdId, adaptiveSize, AdPosition.Bottom);

        //bannerView = new BannerView(bannerAdId , AdSize.Banner, AdPosition.Bottom);

        // Create an empty ad request
        AdRequest request = new AdRequest();

        // Load the banner with the request
        bannerView.LoadAd(request);
    }

    public void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }

    public void ShowBanner()
    {
        if (bannerView != null)
        {
            RequestBanner();
            bannerView.Show();
        }
        else
        {
            RequestBanner();
        }
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "NoAdScene")
            HideBanner();
        else
            ShowBanner();
    }



    #region Interstitial

    public void LoadInterstitialAd()
    {
        

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        InterstitialAd.Load(interstitialAdId, new AdRequest(), (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Interstitial failed to load: " + error?.GetMessage());
                return;
            }

            interstitialAd = ad;

            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial closed. Reloading...");
                LoadInterstitialAd();
            };
        });
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial not ready. Loading again...");
            LoadInterstitialAd();
        }
    }

  /*  public void OnGameFinished()
    {
        gamePlayCount++;
        PlayerPrefs.SetInt(GamePlayKey, gamePlayCount);
        PlayerPrefs.Save();

        Debug.Log("Games Played: " + gamePlayCount);

        if (gamePlayCount % 3 == 0)
        {
            ShowInterstitialAd();
        }
    }*/

    public void OnGameFinished(GameMode mode)
    {
        string key = "GamePlayCount_" + mode.ToString();
        int count = PlayerPrefs.GetInt(key, 0) + 1;
        PlayerPrefs.SetInt(key, count);
        PlayerPrefs.Save();

        Debug.Log($"Games Played in {mode}: {count}");

        if (count % 3 == 0)
        {
            ShowInterstitialAd();
        }
    }

    public enum GameMode { AI, Multiplayer }
    #endregion
}


