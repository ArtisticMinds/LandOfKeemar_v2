using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    public static bool mainMenuOpen;
    public GameObject areYouSyrePanel;
    public GameObject []panels;
    public static MainMenu instance;
    static GameObject mainPanel;
    public TMP_Dropdown qualityDropDown;
    public MissionProgress missionProgress_inMenu;
    public static EventSystem eSystem;

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
        eSystem = FindObjectOfType<EventSystem>();
    }
    private void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;





        foreach (GameObject pan in panels)
            pan.SetActive(false);
        //Add listener for when the state of the Toggle changes, to take action
        qualityDropDown.onValueChanged.AddListener(delegate {
            QualityValueChanged(qualityDropDown);
        });

        LoadOptionsState();

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




    public void SaveOptionsState()
    {
        PlayerPrefs.SetFloat("SoundsVolume", AudioManager.instance.soundsValue);
        PlayerPrefs.SetFloat("MusicsVolume", AudioManager.instance.musicValue);

    }

    public void LoadOptionsState()
    {
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
