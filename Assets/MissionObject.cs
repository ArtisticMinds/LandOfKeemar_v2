using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class MissionObject : MonoBehaviour
{
    public int missionID;
    public Animator []anim;
    public string []animatorTriggerOnTouch;
    public string animatorTriggerToOtheCollisior;
    public UnityEvent onTriggerEnterEvent;


    public void OnTouchActivation()
    {
        if (anim.Length>0)
        {
            for (int i = 0; i < anim.Length; i++)
            {
                anim[i].SetTrigger(animatorTriggerOnTouch[i]);
                Debug.Log(gameObject.name + "OnTouchActivation: " + anim[i] + animatorTriggerOnTouch[i]);
            }
        }
        else
        {
            Debug.Log(gameObject.name+" NO animator");
        }
    }

    public void ToOtherActivation(MissionObject activator)
    {

        if (anim.Length > 0)
        {
            foreach (Animator an in anim)
                an.SetTrigger(activator.animatorTriggerToOtheCollisior);

            Debug.Log(gameObject.name + "ToOtherActivation by: " + activator.gameObject.name+ "Trigger: "+ activator.animatorTriggerToOtheCollisior);
        }
        else
        {
            Debug.Log(gameObject.name + " NO animator. Anctivator:" + activator.gameObject.name);
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (animatorTriggerToOtheCollisior.Equals(string.Empty)) return;

        if (other.CompareTag("InteractiveObject"))
        {
            MissionObject obj = other.GetComponent<MissionObject>();
            obj.ToOtherActivation(this);

            onTriggerEnterEvent.Invoke();

        }
    }

}

