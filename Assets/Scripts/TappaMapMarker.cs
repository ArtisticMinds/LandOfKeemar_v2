using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TappaMapMarker : MonoBehaviour
{

    public Tappa tappa;
    public static Tappa openTappa;
    public Image completeMarker;
    public GameObject qrImage;
    private Button butt;
    public AudioClip onClickClip;

    private void Awake()
    {
        butt=GetComponent<Button>();
        tappa.FindReferences();
        tappa.ResetScriptableObject();
        LoadTappaMissionsProgress();
        butt.onClick.AddListener(OnButtonClick);
        qrImage.SetActive(false);
    }

    void Start()
    {
        tappa.InfoTappa_Real.SetActive(false);
        tappa.InfoTappa_Keemar.SetActive(false);

        GetTappaState(); //Da prendere dopo il load dei savegame

    }

    private void OnEnable()
    {
        GetTappaState();
    }

    public void OnButtonClick()
    {
        if (onClickClip)
            AudioManager.instance.PlayAudioClip(onClickClip);
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


    //COMMENTATO PERCHE' Salvataggio fatto in scena (TappaScene)
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
        qrImage.SetActive(!tappa.isOpen);

        tappa.tappaComplete = true;
        foreach (Tappa.Missions miss in tappa.missions)
        {
            if (!miss.missionComplete)
            {
                tappa.tappaComplete = false;
                return;
            }
        }

        if(tappa.tappaComplete == true)
        {
            GetComponent<Image>().sprite = completeMarker.sprite;
        }

        
    }


    //Imposta la tappa da visualizzare nei dettagli, con i relativi bottoni da visualizzare o nascondere
    public void SetTappa()
    {


     //   MapManager.instance.trueStoryButton.onClick.RemoveAllListeners();
     //   MapManager.instance.keemarStoryButton.onClick.RemoveAllListeners();
      //  MapManager.instance.playButton.onClick.RemoveAllListeners();

        Debug.Log("SetTappa: " + tappa.tappaName);
        MapManager.instance.TMP_title.text = tappa.tappaName;
        //  MapManager.instance.OpenTappaInfos(); //V3. Tolto (ora apre il video)
        //MapManager.instance.trueStoryButton.onClick.AddListener(() => { tappa.InfoTappa_Real.SetActive(true); });
        //MapManager.instance.keemarStoryButton.onClick.AddListener(() => { tappa.InfoTappa_Keemar.SetActive(true); });

        //if (tappa.tappaScene != string.Empty)
        //{
        //    MapManager.instance.playButton.interactable = true;
        //    MapManager.instance.playButton.onClick.AddListener(() =>
        //    {
        //        MainMenu.instance.CloseMainMenu();
        //        AudioManager.instance.StopMusic();
        //        SceneLoader.instance.LoadScene(tappa.tappaScene);

        //        Debug.Log("SetTappa: " + tappa.tappaScene);
        //    });
        //}
        //else
        //{
        //    MapManager.instance.playButton.interactable = false;
        //}

        if (tappa.tappaScene != string.Empty)
        {   
            openTappa = tappa;
            SchedaTappa.instance.playButton.gameObject.SetActive(tappa.isOpen);
            SchedaTappa.instance.playButtonChiusa.gameObject.SetActive(!tappa.isOpen);
            MainMenu.instance.OpenVideoPanel();
        }
        else 
        { 
            Debug.LogWarning("Tappa " + tappa.tappaName + " non ha una scena associata!");
            openTappa = null;
        }


       
    }








}
