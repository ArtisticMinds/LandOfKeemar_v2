using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName= "tappa", menuName="")]
public class Tappa : ScriptableObject
{

    [Multiline]
    public string tappaName;
    public string InfoTappa_Real_Name;
    public string InfoTappa_Keemar_Name;
    [HideInInspector]
    public GameObject InfoTappa_Real;
    [HideInInspector]
    public GameObject InfoTappa_Keemar;
    [Header("Tappe links")]
    public string googleMapLink;
    public string videoLink;
    public static TappaInfos openTappa;
    public bool tappaComplete;
    public string tappaScene;



    [Space(10)]
    public Missions[] missions;

    [Serializable]
    public class Missions
    {
        public string missionName;
        public string missionDescriprion;
        public string missionCompleteMessage;
        public bool missionComplete;
    }


  

    public void FindReferences()
    {
        if (!InfoTappa_Real)
            InfoTappa_Real = FindByName(InfoTappa_Real_Name);

        if (!InfoTappa_Keemar)
            InfoTappa_Keemar = FindByName(InfoTappa_Keemar_Name);
    }

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
}
