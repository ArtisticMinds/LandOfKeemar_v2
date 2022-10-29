using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private static readonly int OpenDoor = Animator.StringToHash("OpenDoor");
    public AudioSource hitPagliaio;
    public AudioSource closeDoor;

    private void OnTriggerEnter(Collider other)
    {
        //print(",,");
        if (other.tag.Equals("Door"))
        {
            other.GetComponent<Animator>().SetTrigger(OpenDoor);
            closeDoor.Play();
        }

        if (other.name.Equals("SM_straw_box"))
            hitPagliaio.Play();
    }
}