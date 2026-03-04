using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Video;
using UnityEngine.UI;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    public static bool mainMenuOpen;
    public GameObject areYouSyrePanel;
    public GameObject QR_ScanPanel;
    public GameObject mapPanel;
    [Header("Parametri video")]
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;
    public GameObject backButtonFromVideo;
    public GameObject fadeVideo;
    public GameObject bookSensitiveAreas;
    public Button bookLeft;
    public Button bookRight;
    public GameObject bookNamesContainer;
    public TMP_Text nomeReale;
    public TMP_Text nomeStoria;
    public SchedaTappa schedaTappa;
    [Space(5)] 
    public GameObject []panels;
    public static MainMenu instance;
    static GameObject mainPanel;
    public TMP_Dropdown qualityDropDown;
    public GameObject qualityMessage;
    public MissionProgress missionProgress_inMenu;
    public static EventSystem eSystem;
    [Space(10)]
    [Header("Tutti i collezionabili nel gioco (UI prefab dallo stesso nome)")]
    public List<GameObject> allCollectablesObject= new List<GameObject>();
    [Header("I nomi dei collezionabili raccolti dal giocatore")]
    public List<string> oggettiRaccolti = new List<string>();
    public GameObject messaggioNoObjects;
    public Transform objectUI_parent;
    private void Awake()
    {
        #region Singleton

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject); //Con questa istruzione rendo "permanente" questo GameObject
        }
        else
        {
            Destroy(transform.root.gameObject);
            return;
        }

        #endregion
        mainMenuOpen = true;
        mainPanel = transform.GetChild(0).gameObject;
        eSystem = FindFirstObjectByType<EventSystem>();
        nomeReale.text = "";
        nomeStoria.text = "";

    }
    private void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        QR_ScanPanel.gameObject.SetActive(false);
        fadeVideo.SetActive(false);
        bookNamesContainer.SetActive(false);

        CloseAllPanels();

        //Add listener for when the state of the Toggle changes, to take action
        qualityDropDown.onValueChanged.AddListener(delegate {
            QualityValueChanged(qualityDropDown);
        });

        bookLeft.GetComponent<Button>().onClick.AddListener(() => { ClickOnLeftBook(); });
        bookRight.GetComponent<Button>().onClick.AddListener(() => { ClickOnRighttBook(); });
        LoadOptionsState();
        LoadOggettiRaccolti();


        

        AudioManager.instance.PlayMenuMusic();
    }

#if UNITY_EDITOR
    [MenuItem("Tools/ResetScriptables")]
    public static void ResetScriptables()
    {
        foreach (Tappa tp in GetAllInstances<Tappa>())
            tp.ResetScriptableObject();
    }

    public static List<T> GetAllInstances<T>() where T : ScriptableObject
    {
        return AssetDatabase.FindAssets($"t: {typeof(T).Name}").ToList()
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<T>)
                    .ToList();
    }
#endif

    public void CloseAllPanels()
    {
        foreach (GameObject pan in panels)
            pan.SetActive(false);
    }

    public void OpenVideoPanel()
    {    

        schedaTappa.gameObject.SetActive(false);
        bookSensitiveAreas.SetActive(false);
        videoPlayer.gameObject.SetActive(false);
        videoPanel.gameObject.SetActive(true);
        fadeVideo.SetActive(true);
        videoPlayer.GetComponentInChildren<RawImage>().color = Color.black * 0;
        backButtonFromVideo.SetActive(false);
        StartCoroutine(OpenVideoRoutine());
        
    }

  

    IEnumerator OpenVideoRoutine()
    {
        Debug.Log("Start OpenVideoRoutine");
        videoPlayer.frame = 1;
        yield return new WaitForSeconds(1);
        videoPlayer.gameObject.SetActive(true);
        // prepare asynchronously
        if (!videoPlayer.isPrepared)
            videoPlayer.Prepare();
        yield return new WaitForSeconds(1);
        nomeReale.text = TappaMapMarker.openTappa.nomeReale;
        nomeStoria.text = TappaMapMarker.openTappa.nomeNeiRomanzi;
        yield return new WaitForSeconds(1);
        videoPlayer.GetComponentInChildren<RawImage>().color = Color.white;


        fadeVideo.GetComponent<Animator>().SetTrigger("FadeOut");
        VideoDebug();
        yield return new WaitForSeconds(2);
        if (!videoPlayer.isPlaying) videoPlayer.Play();

        yield return new WaitForSeconds(2);
        if (!videoPlayer.isPlaying) videoPlayer.Play();
        bookNamesContainer.SetActive(true);
        bookSensitiveAreas.SetActive(true);
        Debug.Log("End OpenVideoRoutine");
        yield return new WaitForSeconds(2);
        backButtonFromVideo.SetActive(true);
        fadeVideo.SetActive(false);
    }

    public void CloseBookPanel()
    {
        bookNamesContainer.SetActive(false);
        videoPlayer.frame = 1;
        videoPlayer.GetComponentInChildren<RawImage>().color = Color.black * 0;
        videoPanel.gameObject.SetActive(false);
        Debug.Log("CloseBookPanel");


    }
    void VideoDebug()
    {
        videoPlayer.errorReceived += (s, msg) => Debug.LogError("[VP] error: " + msg);
        videoPlayer.prepareCompleted += (s) => Debug.Log("[VP] prepared");
        videoPlayer.started += (s) => Debug.Log("[VP] started");
        videoPlayer.frameReady += (s, frame) => Debug.Log("[VP] frameReady " + frame);
    }

    public void ClickOnLeftBook()
    {
        schedaTappa.gameObject.SetActive(true);
        schedaTappa.SetReal();
        bookSensitiveAreas.SetActive(false);
        Debug.Log("Left Book Clicked (real)");
    }
    public void ClickOnRighttBook()
    {
        schedaTappa.gameObject.SetActive(true);
        schedaTappa.SetRomanzi();
        bookSensitiveAreas.SetActive(false);
        Debug.Log("Right Book Clicked (books)");
    }


    public void SaveOptionsState()
    {
        PlayerPrefs.SetFloat("SoundsVolume", AudioManager.instance.soundsValue);
        PlayerPrefs.SetFloat("MusicsVolume", AudioManager.instance.musicValue);

    }

    public void LoadOptionsState()
    {

        qualityMessage.SetActive(false);

        if (PlayerPrefs.HasKey("SoundsVolume"))
        {
            float vol = PlayerPrefs.GetFloat("SoundsVolume");
            AudioManager.instance.SetSoundsVolume(vol);
            AudioManager.instance.SoundsSlider.value = vol;
        }

        if (PlayerPrefs.HasKey("MusicsVolume"))
        {
            float vol = PlayerPrefs.GetFloat("MusicsVolume");
            AudioManager.instance.SetMusicVolume(vol);
            AudioManager.instance.MusicSlider.value = vol;
        }
        if (PlayerPrefs.HasKey("Quality"))
        {
            int qualityLevel = PlayerPrefs.GetInt("Quality");
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality"));
            qualityDropDown.value = qualityLevel;

        }

        qualityMessage.SetActive(qualityDropDown.value >= 1);
    }

    public void LoadOggettiRaccolti()
    {
        oggettiRaccolti.Clear();
        foreach (GameObject obj in allCollectablesObject)
        {
            if (PlayerPrefs.HasKey(obj.name))
                oggettiRaccolti.Add(obj.name);
        }

        SetObjectPanel();
    }

    public void SetObjectPanel()
    {
        if (!objectUI_parent) return;

        messaggioNoObjects.SetActive(oggettiRaccolti.Count.Equals(0));

        foreach (GameObject obj in allCollectablesObject) 
        {
            obj.GetComponent<CanvasGroup>().alpha = 0.1F;
            obj.GetComponent<CanvasGroup>().interactable = false;

            foreach (string str in oggettiRaccolti)
                if (str.Equals(obj.name))
                {
                    obj.GetComponent<CanvasGroup>().alpha = 1F;
                    obj.GetComponent<CanvasGroup>().interactable = true;
                }
                  
        }

    }

    public void OpenKeemarUrl()
    {
        Application.OpenURL("https://www.keemar.it/llibri/");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("CLOSE GAME");
    }

    public void OpenMainMenu() { mainPanel.SetActive(true); mainMenuOpen = true; }

    public void CloseMainMenu() {

        missionProgress_inMenu.CloseMissionProgress();
        mainPanel.SetActive(false);  //Disattivo il menu
        mainMenuOpen = false;
        Debug.Log("CloseMainMenu: " + mainMenuOpen);
    }


    void QualityValueChanged(TMP_Dropdown change)
    {
        Debug.Log("New Value : " + change.value);
        QualitySettings.SetQualityLevel(change.value);
        PlayerPrefs.SetInt("Quality", change.value);

        qualityMessage.SetActive(change.value >= 1); 
    }



    void Update()
    {
        //Alla pressione del tasto ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(mainMenuOpen)
            areYouSyrePanel.SetActive(true);
        }
    }

}
