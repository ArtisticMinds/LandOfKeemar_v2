using UnityEngine;
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
    public GameObject missionCompleteMessage;
    public TMP_Text missionCompleteTextMessage;
    public TMP_Text missionCompleteTitle;
    [Space(10)]
    public GameObject tappaCompleteMessage;
    public TMP_Text tappaCompleteTextMessage;
    public TMP_Text tappaCompleteTitle;


    private float rotSpeed = 20;
    private void Awake()
    {
        TappaMapMarker.openTappa = tappa;
        tappa.ResetScriptableObject();
        LoadTappaState();
        tappaCompleteMessage.SetActive(false);
    }

    public void LoadTappaState()
    {
        foreach (Tappa.Missions mis in TappaMapMarker.openTappa.missions)
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
        {
            AudioManager.instance.PlayMusicClip(bkMusic);
            AudioManager.instance.musicSource.loop = true;
        }

        if (TappaMapMarker.openTappa)
        {
            foreach (Tappa.Missions mis in TappaMapMarker.openTappa.missions)
                Debug.Log(mis.missionName + " Complete:" + mis.missionComplete);

            missions = TappaMapMarker.openTappa.missions;
        }

        tutorialPanel.SetActive(showTutorial);

        CheckTappaCompleted();
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

        StartCoroutine(_MissioneCompletata(missionID));
    }
    System.Collections.IEnumerator _MissioneCompletata(int missionID)
    {
        yield return new WaitForSeconds(1);
        Tappa.Missions mission = missions[missionID];
        mission.missionComplete = true;
        missionCompleteMessage.SetActive(true);
        missionCompleteTextMessage.text = mission.missionCompleteMessage;
        missionCompleteTitle.text = mission.missionName;
        PlayerPrefs.SetInt(mission.missionName, 1);

        if (CheckTappaCompleted())
        {
            StartCoroutine(ShowCompletedTappa());
        }
    }
    System.Collections.IEnumerator ShowCompletedTappa()
    {
        yield return new WaitForSeconds(2);
        tappaCompleteMessage.SetActive(true);
        tappaCompleteTitle.text = tappa.tappaName;
    }

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
}

