using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TappaInfos : MonoBehaviour
{



    public Tappa tappa;
    public static Tappa openTappa;

    private void Awake()
    {
        tappa.FindReferences();
        LoadTappaMissionsProgress();
    }

    void Start()
    {
        tappa.InfoTappa_Real.SetActive(false);
        tappa.InfoTappa_Keemar.SetActive(false);

        GetTappaState(); //Da prendere dopo il load dei savegame

    }
    void LoadTappaMissionsProgress() //Carica lo stato delle missioni di questa tappa
    {
        foreach (Tappa.Missions miss in tappa.missions)
        {
            if (PlayerPrefs.HasKey(miss.missionName)) //Basta che esista
            {
                miss.missionComplete = true;
            }
        }

    }


    //COMMENTATO PERCHE' Salvataggio fatto in scena
    //void SaveTappaProgress() //Salva lo stato di tutte le missioni di questa tappa
    //{
    //    foreach (Tappa.Missions miss in tappa.missions)
    //    {
    //        if (miss.missionComplete)
    //        {
    //            PlayerPrefs.SetString(miss.missionName, "s");//Indifferente il valore
    //        }
    //    }
    //}

    public void GetTappaState()
    {
        tappa.tappaComplete = true;
        foreach (Tappa.Missions miss in tappa.missions)
        {
            if (!miss.missionComplete)
            {
                tappa.tappaComplete = false;
                return;
            }
        }
    }

public void SetTappa()
    {

        //MapManager.instance.trueStoryButton.onClick.RemoveAllListeners();
        //MapManager.instance.keemarStoryButton.onClick.RemoveAllListeners();
        //MapManager.instance.googleMapButton.onClick.RemoveAllListeners();
        //MapManager.instance.videoButton.onClick.RemoveAllListeners();
        //MapManager.instance.playButton.onClick.RemoveAllListeners();

        Debug.Log("SetTappa: " + tappa.tappaName);
        MapManager.instance.TMP_title.text = tappa.tappaName;
        MapManager.instance.OpenTappaInfos();
        MapManager.instance.trueStoryButton.onClick.AddListener(() => { tappa.InfoTappa_Real.SetActive(true); });
        MapManager.instance.keemarStoryButton.onClick.AddListener(() => { tappa.InfoTappa_Keemar.SetActive(true); });

        //if (tappa.tappaScene != string.Empty)
        //{
            MapManager.instance.playButton.onClick.AddListener(() =>
            {
                SceneLoader.instance.LoadScene(tappa.tappaScene);
                MainMenu.instance.CloseMainMenu();
                AudioManager.instance.StopMusic();
            });
       // }


      //  if (tappa.googleMapLink != string.Empty)
            MapManager.instance.googleMapButton.onClick.AddListener(() => { 
                Application.OpenURL(tappa.googleMapLink);
                Debug.Log(MapManager.instance.googleMapButton.onClick.ToString());
            });

        if(tappa.videoLink !=string.Empty)
        MapManager.instance.videoButton.onClick.AddListener(() => { 
            Application.OpenURL(tappa.videoLink); 
        });

        openTappa = tappa;
    }

}
