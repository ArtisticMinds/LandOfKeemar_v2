using UnityEngine;
using TMPro;


[ExecuteInEditMode]
public class GetBuildVersion : MonoBehaviour
{
   public static GetBuildVersion instance;

    private void Awake()
    {
        instance = this;
    }
    public void UpdateVersionText()
    { 
        GetComponent<TMP_Text>().text = "v" + Application.version;
    }

    private void Update()
    {
        UpdateVersionText();
    }
}


