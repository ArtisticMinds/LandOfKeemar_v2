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
        dText = transform.GetChild(0).GetComponent<TMP_Text>();
        text01 = transform.GetChild(1).GetComponent<TMP_Text>();
        text02 = transform.GetChild(2).GetComponent<TMP_Text>();
        text03 = transform.GetChild(3).GetComponent<TMP_Text>();
        gameObject.SetActive(active);
        dText.text = "";

    }

    public static void Log(string message, bool clear = false)
    {
    if(clear)
        dText.text = message + "\n";
     else
        dText.text += message + "\n";
    }
    public void Clear()
    {
        dText.text = "";
    }

    public void Log(string message)
    {
        dText.text = message;
    }
}
