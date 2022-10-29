using UnityEngine;

public class AnimatorEventsUtility : MonoBehaviour
{
    public void DisableAnimator()
    {
        GetComponent<Animator>().enabled = false;
    }

    public void DisableGameObject()
    {
        gameObject.SetActive(false);
    }
}
