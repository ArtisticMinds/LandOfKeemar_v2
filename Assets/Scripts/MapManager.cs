using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MapManager : MonoBehaviour
{

    public static MapManager instance;
    public RectTransform rectTransform;
    public float zoomSpeed = 0.7f;
    public Animator anim;
    public Button zoomInButton;
    public Button zoomOutButton;
    public GameObject tappaMainPanel;
    public GameObject mapIndex;
    public GameObject tappaTutorial;

    [SerializeField]
    static float zoomValue = 1;
    [SerializeField]
    static int zState = 0;

    public TMP_Text TMP_title;
    public Button infosButton;
    public Button playButton;
    public Button trueStoryButton;
    public Button keemarStoryButton;
    public Button googleMapButton;
    public Button videoButton;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        CloseTappaInfos();
        zState = 0;
    }

    public void OpenTappaInfos()
    {
         anim.SetBool("OpenTappaInfos", true);

         zoomInButton.interactable = zoomOutButton.interactable = false;
         mapIndex.SetActive(false);
         tappaTutorial.SetActive(true);
    }

    public void CloseTappaInfos()
    {
        anim.SetBool("OpenTappaInfos", false);
        zoomInButton.interactable = zoomOutButton.interactable = true;
        mapIndex.SetActive(true);
        tappaTutorial.SetActive(false);
        TappaMapMarker.openTappa = null;
    }

    #region Zoom
    public void ZoomIn()
    {
        zState = 1;
    }
    public void ZoomOut()
    {
        zState = 2;
    }

    public void ZoomStop()
    {
        zState = 0;
    }

    void Zoom()
    {

        if (zState == 1)
        {
            zoomValue += Time.unscaledDeltaTime * zoomSpeed;
            rectTransform.localScale = Vector3.one * zoomValue;
        }
        else if (zState == 2)
        {
            zoomValue -= Time.unscaledDeltaTime * zoomSpeed;
            rectTransform.localScale = Vector3.one * zoomValue;
        }

        zoomValue = Mathf.Clamp(zoomValue, 1f, 2.5f);
    }

    #endregion

    void Update()
    {

        Zoom();


    }
}