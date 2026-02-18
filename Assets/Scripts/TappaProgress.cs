using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class TappaProgress : MonoBehaviour
{
    public TMP_Text tappaTitle;
    public GameObject icon;
    public Color completeColor= new Color(0.5F, 0.7F, 0.1F, 1);
    public GameObject tappaDescription;
    public TMP_Text tappaDescriptionText;


    void Awake()
    {
        SetIncomplete();
        tappaDescription.SetActive(false);

    }


    [ContextMenu("Set Incomplete")]
    public void SetIncomplete()
    {
        tappaTitle.color = new Color(0.2F, 0.2F, 0.1F, 0.5F);
        icon.SetActive(false);
    }


    [ContextMenu("Set Complete")]
    public void SetComplete()
    {
        tappaTitle.color = completeColor;
        icon.SetActive(true);
    }
}
