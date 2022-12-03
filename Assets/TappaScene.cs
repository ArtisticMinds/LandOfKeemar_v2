using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;


public class TappaScene : MonoBehaviour
{
 
    public AudioClip bkMusic;
    public Tappa.Missions[] missions;

    private bool showTutorialAtStart;
    public Tappa tappa;

    private float rotSpeed = 20;

    private void Awake()
    {
        if (MainMenu.eSystem)
        {
            foreach (EventSystem es in FindObjectsOfType<EventSystem>())
                if(es!= MainMenu.eSystem)
                Destroy(es.gameObject);
        }

        if (!InGameCanvas.instance) InGameCanvas.instance = FindObjectOfType<InGameCanvas>();

        TappaMapMarker.openTappa = tappa;
        tappa.ResetScriptableObject();
        LoadTappaState();
        InGameCanvas.instance.tappaCompleteMessage.SetActive(false);
        AudioManager.instance.Initialize();
    
    }

 
    public void LoadTappaState()
    {
        foreach (Tappa.Missions mis in TappaMapMarker.openTappa.missions)
        {
            mis.missionComplete = PlayerPrefs.HasKey(mis.missionName);
        }
    }


    void OnMouseDrag()
    {
        float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
        float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

        transform.Rotate(Vector3.up, -rotX);
        transform.Rotate(Vector3.right, rotY);
    }

    void Start()
    {
        DebugConsole.Log("Start");
        if (AudioManager.instance)
        {
            AudioManager.instance.PlayMusicClip(bkMusic);
        }

        if (TappaMapMarker.openTappa)
        {
            foreach (Tappa.Missions mis in TappaMapMarker.openTappa.missions)
                DebugConsole.Log(mis.missionName + " Complete:" + mis.missionComplete);

            missions = TappaMapMarker.openTappa.missions;
        }

        DebugConsole.Log("TappaMapMarker.openTappa:" + TappaMapMarker.openTappa.tappaName);

        InGameCanvas.instance.allMissionCompleteMessage.SetActive(CheckTappaCompleted());


        //Primo avvio
        if (!PlayerPrefs.HasKey("FirstStart"))
        {
            showTutorialAtStart = true;
            PlayerPrefs.SetInt("FirstStart", 1);
        }
        else
        {
            showTutorialAtStart = false;
        }
        InGameCanvas.instance.tutorialPanel.SetActive(showTutorialAtStart);

        SetQuality();
    }

    void SetQuality()
    {
        if (QualitySettings.GetQualityLevel() == 0)
            InteractionManager.sceneCam.farClipPlane = 25;
        else if (QualitySettings.GetQualityLevel() == 1)
            InteractionManager.sceneCam.farClipPlane = 35;
        else if (QualitySettings.GetQualityLevel() == 2)
            InteractionManager.sceneCam.farClipPlane = 45;
    }


    public void MissioneCompletata(int missionID)
    {
        DebugConsole.Log("MissioneCompletata");
        StartCoroutine(_MissioneCompletata(missionID));
    }
    System.Collections.IEnumerator _MissioneCompletata(int missionID)
    {
        yield return new WaitForSeconds(1);
        DebugConsole.Log("InGameCanvas.instance:"+ InGameCanvas.instance.name);
        Tappa.Missions mission = missions[missionID];
        mission.missionComplete = true;
        InGameCanvas.instance.missionCompleteMessage.SetActive(true);
        InGameCanvas.instance.missionCompleteTextMessage.text = mission.missionCompleteMessage;
        InGameCanvas.instance.missionCompleteTitle.text = mission.missionName;
        PlayerPrefs.SetInt(mission.missionName, 1); //Salva missione completata
        FindObjectOfType<MissionProgress>().CloseMissionProgress();
        if (CheckTappaCompleted())
        {
            StartCoroutine(ShowCompletedTappa());
        }
    }

    System.Collections.IEnumerator ShowCompletedTappa()
    {
        yield return new WaitForSeconds(3);
        InGameCanvas.instance.tappaCompleteMessage.SetActive(true);
        InGameCanvas.instance.tappaCompleteTitle.text  = tappa.tappaName.Replace("\n", "").Replace("\r", "");
        //  PlayerPrefs.SetInt(tappa.tappaName, 1); //Salva Tappa completata (per ora non serve, basta CheckTappaCompleted())
    }


    //Controlla e ritorna se almeno una missione di questa tappa è stata completata 
    public bool CheckOneMissionCompleted()
    {
        bool oneMissionCompleted=false;
        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i].missionComplete)
                oneMissionCompleted= true;

        }
        return oneMissionCompleted;
    }

    //Controlla e ritorna se la tappa è completa (tutte le missioni complete)
    public bool CheckTappaCompleted()
    {
        tappa.tappaComplete = true;

        for (int i = 0; i< missions.Length; i++)
        {
            if (!missions[i].missionComplete)
            {
                tappa.tappaComplete = false;

            }
        }
        return tappa.tappaComplete;
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        AudioManager.instance.soundsSource.PlayOneShot(audioClip);
    }
}

