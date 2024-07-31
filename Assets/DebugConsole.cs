using TMPro;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public bool active = true;
    static TMP_Text dText;
    static public TMP_Text text01;
    static public TMP_Text text02;
    static public TMP_Text text03;
    private void Awake()
    {
        if (transform.childCount > 0) dText = transform.GetChild(0).GetComponent<TMP_Text>();
        if (transform.childCount > 1) text01 = transform.GetChild(1).GetComponent<TMP_Text>();
        if (transform.childCount>2) text02 = transform.GetChild(2).GetComponent<TMP_Text>();
 
        
        dText.text = "";

    }

    private void Start()
    {
        gameObject.SetActive(active);
    }

    public static void Log(string message, bool clear = false)
    {
        if (!dText) return;

    if(clear)
        dText.text = message + "\n";
     else
        dText.text += message + "\n";
    }
    public void Clear()
    {
        if (!dText) return;
        gameObject.SetActive(active);
        dText.text = "";
    }

    public void Log(string message)
    {
        if (!dText) return;
        gameObject.SetActive(active);
        dText.text = message;
    }
}
