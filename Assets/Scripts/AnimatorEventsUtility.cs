using UnityEngine;

public class AnimatorEventsUtility : MonoBehaviour
{
    //Se isKinematic è vero, non fa niente, serve solo per avere questo script come ricever dell'evento e non avere l'errore
    //quando l'animazione è condivisa con altri oggetti che devono attivare l'evento
    public bool isKinematic;


    public ParticleSystem emitParticles;
    public AudioClip _audioClip;
    public GameObject showAtEnd;
    public GameObject showObject;
    public Animator setTrigger;
    public GameObject[] hideGameObjects;

    public string sendMessage;
    public GameObject toObject;

    public void DisableAnimator()
    {
        if (isKinematic) return;

        GetComponent<Animator>().enabled = false;
    }

    public void DisableGameObject()
    {
        if (isKinematic) return;

        gameObject.SetActive(false);
    }

    public void EmitParticlesFromInspector()
    {
        if (isKinematic) return;

        if (emitParticles)
        emitParticles.Play();
        else
        Debug.Log("Animation Event have no emitParticles");
    }
    public void PlayAudioClip(AudioClip audioClip)
    {
        if (isKinematic) return;

        AudioManager.instance.soundsSource.PlayOneShot(audioClip);
    }

    public void PlayAudioClipFromInspector()
    {
        if (isKinematic) return;

        if (_audioClip)
            AudioManager.instance.soundsSource.PlayOneShot(_audioClip);
        else
            Debug.Log("No _audioClip");
    }

    public void ShowonAnimationEnd()
    {
        if (isKinematic) return;

        if (showAtEnd)
        showAtEnd.SetActive(true);
    }

    public void ShowonObject()
    {
        if (isKinematic) return;

        if (showObject)
            showObject.SetActive(true);
    }

    public void HideGameObject()
    {
        if (isKinematic) return;

        if (hideGameObjects.Length>0)
            foreach(GameObject gobj in hideGameObjects)
                gobj.SetActive(false);
    }

    public void SendMessageTo()
    {
        if (isKinematic) return;
        if (toObject)
        toObject.SendMessage(sendMessage);
    }

    public void SetTriggerTo(string trigger)
    {
        if (isKinematic) return;
        if (trigger!=string.Empty)
        setTrigger.SetTrigger(trigger);
    }

    public void CompleteMission(int idMission)
    {
        if (isKinematic) return;
        InGameCanvas.tappaScene.MissioneCompletata(idMission);
    }
}
