using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class InGameCanvas : MonoBehaviour
{
    public static InGameCanvas instance;
    public GameObject tutorialPanel;
    [Space(10)]
    public GameObject missionCompleteMessage;
    public TMP_Text missionCompleteTextMessage;
    public TMP_Text missionCompleteTitle;
    [Space(10)]
    public GameObject tappaCompleteMessage;
    public TMP_Text tappaCompleteTextMessage;
    public TMP_Text tappaCompleteTitle;
    public GameObject allMissionCompleteMessage;
    public static TappaScene tappaScene;
    public static CameraController cameraController;
    [Space(10)]
    public GameObject oggettoRaccoltoMessage;
    public TMP_Text oggettoRaccoltoName;

    private void Awake()
    {
        instance = this;
        tappaScene = FindObjectOfType<TappaScene>();
        cameraController = FindObjectOfType<CameraController>();
    }

    public void ReturnToMenu()
    {
        MainMenu.instance.OpenMainMenu();
        AudioManager.instance.PlayMenuMusic();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
        MapManager.instance.CloseTappaInfos();
        MainMenu.instance.SetObjectPanel();

    }

    public void RestartMission(bool clearSave)
    {
        foreach (Tappa.Missions mission in tappaScene.missions)
        {
            mission.missionComplete = false;
            if (clearSave)
                PlayerPrefs.DeleteKey(mission.missionName);
        }

        AudioManager.instance.StopMusic();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        if (SceneLoader.instance)
            SceneLoader.instance.LoadScene(tappaScene.tappa.tappaScene);

        Debug.Log("SetTappa: " + tappaScene.tappa.tappaScene);
    }


    public void SetTopView()
    {
        cameraController.TopView();
    }

    public void HorizontalScroll(float direction)
    {

        cameraController.HorizontalScroll(direction);
        InteractionManager.instance.SceneObjectsInteractions(false);

    }

    public void VerticalScroll(float direction)
    {

        cameraController.VerticalScroll(direction);
        InteractionManager.instance.SceneObjectsInteractions(false);

    }
    public void EndVscroll()
    {
        cameraController.EndVscroll();
        InteractionManager.instance.SceneObjectsInteractions(true);
    }
    public void EndHscroll()
    {
        cameraController.EndHscroll();
        InteractionManager.instance.SceneObjectsInteractions(true);

    }
}
