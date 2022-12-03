using UnityEngine;

public class AnimatorEventsUtility : MonoBehaviour
{
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
        GetComponent<Animator>().enabled = false;
    }

    public void DisableGameObject()
    {
        gameObject.SetActive(false);
    }

    public void EmitParticlesFromInspector()
    {
        if(emitParticles)
        emitParticles.Play();
        else
        Debug.Log("Animation Event have no emitParticles");
    }
    public void PlayAudioClip(AudioClip audioClip)
    {
       AudioManager.instance.soundsSource.PlayOneShot(audioClip);
    }

    public void PlayAudioClipFromInspector()
    {
        if (_audioClip)
            AudioManager.instance.soundsSource.PlayOneShot(_audioClip);
        else
            Debug.Log("No _audioClip");
    }

    public void ShowonAnimationEnd()
    {
        if(showAtEnd)
        showAtEnd.SetActive(true);
    }

    public void ShowonObject()
    {
        if (showObject)
            showObject.SetActive(true);
    }

    public void HideGameObject()
    {
        if (hideGameObjects.Length>0)
            foreach(GameObject gobj in hideGameObjects)
                gobj.SetActive(false);
    }

    public void SendMessageTo()
    {
        toObject.SendMessage(sendMessage);
    }

    public void SetTriggerTo(string trigger)
    {
        setTrigger.SetTrigger(trigger);
    }

    public void CompleteMission(int idMission)
    {
        InGameCanvas.tappaScene.MissioneCompletata(idMission);
    }
}
