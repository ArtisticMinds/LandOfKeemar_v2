using UnityEngine;

public class RandomizeAnimation : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Animator>().speed = Random.Range(0.4F, 1F);
    }
}