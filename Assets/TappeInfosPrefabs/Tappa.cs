using System;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[CreateAssetMenu(fileName= "tappa", menuName="")]
public class Tappa : ScriptableObject
{
    public bool isOpen;
    [Multiline]
    public string tappaName;
    public int tappaID;
    public string nomeReale;
    public string nomeNeiRomanzi;
    [Multiline]
    public string storiaReale;
    [Multiline]
    public string storiaRomanzi;
    public AudioClip descrizioneAudioReal;
    public AudioClip descrizioneAudioRomanzi;
    public SpriteCollection slideSpriteCollection;
    public SpriteCollection slideSpriteCollectionFantasy;
    [HideInInspector]
    public GameObject InfoTappa_Real;
    [HideInInspector]
    public GameObject InfoTappa_Keemar;
    [Header("Tappe links")]
    public string googleMapLink;
    public string videoLink;
    public static TappaMapMarker openTappa;
    public bool tappaComplete;
    public string tappaScene;




    [Space(10)]
    public Missions[] missions;

    [Serializable]
    public class Missions
    {
        public string missionName;
        [Multiline]
        public string missionDescriprion;
        public string missionCompleteMessage;
        public bool missionComplete;
    }


    private void Awake()
    {
        ResetScriptableObject();
    }

    public void ResetScriptableObject()
    {
        foreach (Missions mis in missions)
            mis.missionComplete = false;

        Debug.Log("Reset");
    }


    //public void FindReferences()
    //{
    //    if (!InfoTappa_Real)
    //        InfoTappa_Real = FindByName(InfoTappa_Real_Name);

    //    if (!InfoTappa_Keemar)
    //        InfoTappa_Keemar = FindByName(InfoTappa_Keemar_Name);
    //}

    static GameObject FindByName(string goName)
    {
        GameObject go = null;

        foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (obj.name.Equals(goName))
            {
                go = obj;
                return go;
            }
        }

        return null;
    }



    //Apre la tappa, rendendola giocabile (isOpen=true) e salva lo stato (se andSave è true)
    //IsOpen si salva dopo aver scansionato il QR (o altri modi volendo), quindi è giocabile, altrimenti è bloccata (non si può accedere alla scena) ed è attivo il QR per sbloccarla
    public void OpenTappa(bool andSave=true)
    {
        //Metto anche la possibilità di aprire la tappa senza salvare, così da poterla aprire per test senza dover scansionare il QR ogni volta
        if (andSave)
        PlayerPrefs.SetString(tappaName+"_IsOpen", "true");

        // Evitiamo duplicati: se è già aperta non facciamo nulla
        if (isOpen)
        {
            Debug.Log($"Tappa {tappaName} già aperta (skip).");
            return;
        }

        MainMenu.instance.tappeChiuse.Remove(this);
        MainMenu.instance.tappeAperte.Add(this);
        isOpen = true;

        Debug.Log("Tappa " + tappaName + " aperta!");
    }

}
