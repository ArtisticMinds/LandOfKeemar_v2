using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotObject : MonoBehaviour
{
    public Rigidbody shotChildren;
    public float colliderActivationDelay = 1;
    public Vector3 shotForce;
     private Collider coll;
    private Vector3 orPosition;
    private Quaternion orRotation;
    public AudioClip onShotClip;

    public bool shotActive;

    private void Awake()
    {
        coll = shotChildren.GetComponent<Collider>();
        orPosition = coll.transform.localPosition;
        orRotation = coll.transform.localRotation;
        Reload();
    }

    public void EnableShot(bool activate)
    {
        shotActive = activate;
    }

    public void Shot()
    {
        if (!shotActive) return;

        if(onShotClip)
        AudioManager.instance.PlayAudioClip(onShotClip);

        shotChildren.isKinematic = false;
        shotChildren.transform.SetParent(null);
        shotChildren.AddRelativeForce(shotForce,ForceMode.VelocityChange);
        StartCoroutine(ActivateCollider());
    }

    IEnumerator ActivateCollider()
    {
        yield return new WaitForSeconds(colliderActivationDelay);
        coll.enabled = true;
    }

    public void Reload()
    {
        coll.enabled = false;
        shotChildren.isKinematic = true;
        shotChildren.transform.SetParent(transform);
        coll.transform.localPosition = orPosition;
        coll.transform.localRotation = orRotation;
    }
}
