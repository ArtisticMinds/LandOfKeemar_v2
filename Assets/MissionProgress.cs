using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine;

public class MissionProgress : MonoBehaviour
{

    public GameObject[] hideOnOpen;
    public bool progressPanelisOpen;
    public Image progressButton;
    [Space(10)]
    public Transform progressContent;
    public GameObject progressPrefab;



    public void OpenMissionProgress()
    {
        if (progressPanelisOpen)
        {
            CloseMissionProgress();
            return;
        }

       transform.GetChild(0).gameObject.SetActive(true);
        foreach (GameObject obj in hideOnOpen)
            obj.SetActive(false);

        progressPanelisOpen = true;
        progressButton.color = new Color(0.9F, 0.9F, 0.9F, 0.8F);

        SetTappaProgress();
    }

    public void CloseMissionProgress()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        foreach (GameObject obj in hideOnOpen)
            obj.SetActive(true);

        progressPanelisOpen = false;
        progressButton.color = new Color(1F, 1, 1, 1);
    }


    public void SetTappaProgress()
    {
        foreach (Transform tr in progressContent)
        {
            if(tr!= progressContent.GetChild(0))
            Destroy(tr.gameObject);
        }

        if(TappaMapMarker.openTappa)
        foreach (Tappa.Missions miss in TappaMapMarker.openTappa.missions)
        {
           GameObject mission= Instantiate(progressPrefab, progressContent);
            mission.GetComponent<TappaProgress>().tappaTitle.text = miss.missionName;
                mission.GetComponent<TappaProgress>().tappaDescriptionText.text = miss.missionDescriprion;

                if (miss.missionComplete)
            mission.GetComponent<TappaProgress>().SetComplete(); 
        }
            
    }
}
