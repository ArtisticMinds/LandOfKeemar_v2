using TMPro;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public bool active = true;
    static TMP_Text dText;
    private void Awake()
    {
        dText = GetComponentInChildren<TMP_Text>();
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
