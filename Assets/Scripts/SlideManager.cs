using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class SlideManager : MonoBehaviour
{
    [SerializeField]
    int actualSlide;

    [SerializeField]
    Button forwardButton;
    [SerializeField]
    Button backdButton;
    [SerializeField]
    Button endButton;



    [SerializeField]
    List<GameObject> slides = new List<GameObject>();
    void OnEnable()
    {
        backdButton.gameObject.SetActive(false);

        foreach (GameObject sl in slides)
            sl.SetActive(false);

        slides[0].SetActive(true);
        actualSlide = 0;
        UpdateButtons();
    }


    public void Back()
    {
        if (actualSlide > 0)
        {
            slides[actualSlide].SetActive(false);
            slides[actualSlide - 1].SetActive(true);
            actualSlide--;
        }
        else
            backdButton.gameObject.SetActive(false);


        UpdateButtons();
    }


    public void Forward()
    {
        if (actualSlide < slides.Count - 1)
        {
            slides[actualSlide].SetActive(false);
            slides[actualSlide + 1].SetActive(true);
            actualSlide++;
        }
        else
        {
            forwardButton.gameObject.SetActive(false);
          if(endButton) endButton.gameObject.SetActive(true);
        }
        UpdateButtons();

    }

    void UpdateButtons()
    {

        if (actualSlide > 0)
            backdButton.gameObject.SetActive(true);
        else
            backdButton.gameObject.SetActive(false);

        if (actualSlide < slides.Count - 1)
        {
            forwardButton.gameObject.SetActive(true);
            if (endButton) endButton.gameObject.SetActive(false);
        }
        else
        {
            forwardButton.gameObject.SetActive(false);
            if (endButton) endButton.gameObject.SetActive(true);
        }
    }

}
