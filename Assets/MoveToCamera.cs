using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    [SerializeField] float speed = 3;
    [SerializeField] bool active;
    [SerializeField] float destroyTime = 5;
    [SerializeField] ParticleSystem playOnDestroy;
    public float depth;
    [Range(-1F, 3)]
    public float minDepth =1F;
    [Range(-1F, 3)]
    public float maxDepth =1.5F;
    public float endScale=1;
    float timer;

   public void Activate()
    {
        active=true;
    }
    public void Deactivate()
    {
        active=false;
    }
    public void ActivateAndDestroy()
    {
        active = true;
        transform.localRotation = Quaternion.Euler(Vector3.up * 90);
        transform.localScale *= endScale;


    }

    public void PlayParticles()
    {
        if (playOnDestroy)
        {
            playOnDestroy.Play();

        }
    }
    private void Destroy()
    {

        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 5*Time.deltaTime);


        PlayParticles();

        if (transform.localScale.x<=0.1F)
        Destroy(gameObject);
    }

   

    void LateUpdate()
    {
        if (active)
        {
            transform.SetParent(InteractionManager.camController.collectObjectPoint);

            depth = 150F/InteractionManager.camController.sceneCamera.fieldOfView / InteractionManager.camController.maxCameraFieldOfView* InteractionManager.camController.minCameraFieldOfView;
            depth = Mathf.Clamp(depth, minDepth, maxDepth);
            Vector3 endPOint = new Vector3(offset.x, offset.y, offset.z+ depth); ;
            transform.localPosition = Vector3.Slerp(transform.localPosition, endPOint, Time.deltaTime * speed);
            transform.Rotate(0, 2, 0, Space.Self);

            timer += Time.deltaTime;
            if (timer > destroyTime)
                Destroy();
        }
    }
}
