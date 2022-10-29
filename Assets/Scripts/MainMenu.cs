
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public static bool mainMenuOpen;
    public GameObject areYouSyrePanel;
    public GameObject []panels;
    public static MainMenu instance;
    static GameObject mainPanel;

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
    }
    private void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        foreach (GameObject pan in panels)
            pan.SetActive(false);

        
        LoadOptionsState();

        AudioManager.instance.PlayMenuMusic();
    }

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

        MissionProgress.instance.CloseMissionProgress();
        mainPanel.SetActive(false);  //Disattivo il menu
        mainMenuOpen = false;

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
