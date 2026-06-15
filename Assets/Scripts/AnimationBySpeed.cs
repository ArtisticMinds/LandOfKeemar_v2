using UnityEngine;

public class AnimationBySpeed : MonoBehaviour
{
    [Tooltip("Animator che contiene i trigger StartWalk / StopWalk. Se nullo viene preso dal GameObject.")]
    public Animator animator;

    [Tooltip("Nome del trigger da inviare quando l'oggetto inizia a muoversi.")]
    public string startTrigger = "StartWalk";

    [Tooltip("Nome del trigger da inviare quando l'oggetto si ferma.")]
    public string stopTrigger = "StopWalk";

    [Tooltip("Soglia (m/s) sotto la quale si considera l'oggetto fermo. Evita flicker dovuti a precisione float/rumore.")]
    public float speedThreshold = 0.01f;

    [Tooltip("Fattore di smoothing (0 = nessuno, >0 = smoothing della velocità calcolata).")]
    public float velocitySmoothing = 5f;

    Vector3 lastPosition;
    float smoothedSpeed = 0f;
    bool isWalking = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        lastPosition = transform.position;
        smoothedSpeed = 0f;
        isWalking = false;
    }

    void Update()
    {
        if (animator == null) return;

        // calcola velocità istantanea dal delta posizione (l'oggetto è mosso da animazione)
        Vector3 delta = transform.position - lastPosition;
        float rawSpeed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
        lastPosition = transform.position;

        // smoothing opzionale
        float t = Mathf.Clamp01(velocitySmoothing * Time.deltaTime);
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, t);

        bool moving = smoothedSpeed > speedThreshold;

        if (moving && !isWalking)
        {
            // inizio camminata
            animator.ResetTrigger(stopTrigger);
            animator.SetTrigger(startTrigger);
            isWalking = true;
        }
        else if (!moving && isWalking)
        {
            // fine camminata
            animator.ResetTrigger(startTrigger);
            animator.SetTrigger(stopTrigger);
            isWalking = false;
        }
    }

    // utilità: permette di forzare stato (opzionale)
    public void ForceStopped()
    {
        if (animator == null) return;
        animator.ResetTrigger(startTrigger);
        animator.SetTrigger(stopTrigger);
        isWalking = false;
        smoothedSpeed = 0f;
    }

    public void ForceWalking()
    {
        if (animator == null) return;
        animator.ResetTrigger(stopTrigger);
        animator.SetTrigger(startTrigger);
        isWalking = true;
    }
}
