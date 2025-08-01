using TMPro;
using UnityEngine;

public class GetTappaTitle : MonoBehaviour
{

    void Start()
    {
        GetComponent<TMP_Text>().text = FindFirstObjectByType<TappaScene>().tappa.tappaName;
    }

 
}
