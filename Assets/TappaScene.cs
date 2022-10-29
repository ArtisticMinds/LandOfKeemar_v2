using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro;


public class TappaScene : MonoBehaviour
{
    public AudioClip bkMusic;
    public Tappa.Missions[] missions;
    public GameObject tutorialPanel;
    public bool showTutorial;
    public Tappa tappa;

    [Space(10)]
    public GameObject completeMessage;
    public TMP_Text missionCompleteTextMessage;
    public TMP_Text missionCompleteTitle;
 

    [Serializable]
    public class SubMissions
    {
        public string subMissionName;
        public bool subMissionComplete;

    }

    void ResetScriptableObject()
    {
        foreach (Tappa.Missions mis in TappaInfos.openTappa.missions)
            mis.missionComplete = false;
    }
 


    private float rotSpeed = 20;
    private void Awake()
    {
        TappaInfos.openTappa = tappa;
        ResetScriptableObject();
        LoadTappaState();

    }

    public void LoadTappaState()
    {
        foreach (Tappa.Missions mis in TappaInfos.openTappa.missions)
        {
            mis.missionComplete =PlayerPrefs.HasKey(mis.missionName);
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
        if (AudioManager.instance)
            AudioManager.instance.PlayMusicClip(bkMusic);

        if (TappaInfos.openTappa)
        {
            foreach (Tappa.Missions mis in TappaInfos.openTappa.missions)
                Debug.Log(mis.missionName + " Complete:" + mis.missionComplete);

            missions = TappaInfos.openTappa.missions;
        }

        tutorialPanel.SetActive(showTutorial);

        CheckTappaComplete();
    }

    public void ReturnToMenu()
    {
        MainMenu.instance.OpenMainMenu();
        AudioManager.instance.PlayMenuMusic();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
        MissionProgress.instance.CloseMissionProgress();
        MapManager.instance.CloseTappaInfos();


    }

   public void MissioneCompletata(int missionID)
    {
        Tappa.Missions tappa = missions[missionID];
        tappa.missionComplete = true;
        completeMessage.SetActive(true);
        missionCompleteTextMessage.text = tappa.missionCompleteMessage;
        missionCompleteTitle.text = tappa.missionName;
        PlayerPrefs.SetInt(tappa.missionName, 1);
        
    }

    public void CheckTappaComplete()
    {
         for (int i = 0; i< missions.Length; i++)
        {
            if (!missions[i].missionComplete)
            {
                tappa.tappaComplete = false;
            }
        }
    }
}

