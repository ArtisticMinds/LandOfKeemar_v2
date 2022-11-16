using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideByQuality : MonoBehaviour
{
    [SerializeField]
    int qualityLimit = 2;
    void Start()
    {
        SetByQuality();
    }


   public void SetByQuality()
    {
        if(QualitySettings.GetQualityLevel()< qualityLimit)
           Destroy(gameObject);
    }
}
