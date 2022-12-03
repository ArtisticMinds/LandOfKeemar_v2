using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class MissionObject : MonoBehaviour
{
    [Header("Nome che identifica questo oggetto, se è collezionabile")]
    public string collezionabileID;
    public bool enableTouch = true;
    [Header("Animators coinvolti nelle interazione con questo oggetto")]
    public Animator[] anim;
    [Header("Nome trigger su uno degli animator corrispondente per ID")]
    public string[] animatorTriggerOnTouch;

    [Header("Nome trigger sull'animator dell'oggetto colpito da questo")]
    public string animatorTriggerToOtheCollisior;

    [Header("Evento alla collisione con un altro MissionObject")]
    public UnityEvent onTriggerEnterEvent;

    [Header("Evento alla collisione con un altro trigger NON MissionObject")]
    public UnityEvent onDefaultTriggerEnterEvent;
    

    [Header("Evento istantaneo al touch singlo")]
    public UnityEvent onTouchEvent;

    [Header("Evento ritardato (di 1 secondo) al touch singlo")]
    public UnityEvent onTouchEventDalyed;

    [Header("Evento richiamabile da evento animazione")]
    public UnityEvent onAnimationEvent;

    float timer;
    bool touchPause;
    [Header("Fa in modo che dopo il touch, l'interazione sarà disabilitata per pausaTime")]
    public bool useTouchPause; 
    public float pauseTime=2;

    public virtual void OnTouchActivation()
    {
        if (!enableTouch) return;
        if(useTouchPause && touchPause) return;

        if (anim.Length>0)
        {
            for (int i = 0; i < anim.Length; i++)
            {
                if (animatorTriggerOnTouch.Length >0)
                {
                    anim[i].SetTrigger(animatorTriggerOnTouch[i]);
                    Debug.Log(gameObject.name + "OnTouchActivation: " + anim[i] + animatorTriggerOnTouch[i]);
                }
            }
        }
        else
        {
            Debug.Log(gameObject.name+" No animator");
        }

        onTouchEvent.Invoke();
        StartCoroutine(TouchEventDalyed());
        TouchPause(pauseTime);
    }


    public void EnableTouch(bool _enable)
    {
        enableTouch = _enable;
    }

    IEnumerator TouchEventDalyed()
    {
        yield return new WaitForSeconds(1);
        onTouchEventDalyed.Invoke();
    }

    public void ToOtherActivation(MissionObject activator)
    {
        if (animatorTriggerToOtheCollisior.Equals(string.Empty)) return;
        if (anim.Length > 0)
        {
            foreach (Animator an in anim)
                an.SetTrigger(activator.animatorTriggerToOtheCollisior);

            Debug.Log(gameObject.name + " Call ToOtherActivation by: " + activator.gameObject.name+ " - Trigger Name: "+ activator.animatorTriggerToOtheCollisior);
        }
        else
        {
            Debug.Log(gameObject.name + " NO animator. Anctivator:" + activator.gameObject.name);
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("InteractiveObject"))
        {
            OnTriggerEnterEvent();
            MissionObject obj = other.GetComponent<MissionObject>();
           if(obj)obj.ToOtherActivation(this);
      
        }
        else
        {
            OnDefaultTriggerEnterEvent();

        }
    }

    public void OnTriggerEnterEvent() { Debug.Log(transform.name + "CALL OnTriggerEnterEvent"); onTriggerEnterEvent.Invoke(); }
    public void OnDefaultTriggerEnterEvent() { Debug.Log(transform.name + "CALL OnTriggerEnterEvent"); onDefaultTriggerEnterEvent.Invoke(); }
    public void PlayClip(AudioClip audioClip)
    {
        if (AudioManager.instance)
            AudioManager.instance.soundsSource.PlayOneShot(audioClip);
    }

    public void TouchPause(float _pauseTime) {
        touchPause = true;
        pauseTime = _pauseTime;
    }


    public void CollectObject()
    {
        

        if (collezionabileID != string.Empty)
        {
            if (MainMenu.instance && !MainMenu.instance.oggettiRaccolti.Contains(collezionabileID))
                MainMenu.instance.oggettiRaccolti.Add(collezionabileID);
            InGameCanvas.instance.oggettoRaccoltoName.text= collezionabileID;
            InGameCanvas.instance.oggettoRaccoltoMessage.SetActive(true);
            PlayerPrefs.SetString(collezionabileID, collezionabileID);
        }
    }

    public void AnimationEvent()
    {
        onAnimationEvent.Invoke();
    }
 
    private void Update()
    {
        if (!useTouchPause) return;

        if (touchPause)
        {
            timer += Time.deltaTime;
            if (timer > pauseTime)
            {
                touchPause = false;
                timer = 0;
            }
        }
        Debug.Log(touchPause);
    }

}

