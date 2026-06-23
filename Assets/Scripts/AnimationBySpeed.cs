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

    [Header("Idle random speed")]
    [Tooltip("Abilita la randomizzazione della velocità mentre l'animatore è in idle (oggetto fermo).")]
    public bool randomizeIdleSpeed = true;
    [Tooltip("Velocità minima dell'animatore durante l'idle.")]
    public float minIdleSpeed = 0.85f;
    [Tooltip("Velocità massima dell'animatore durante l'idle.")]
    public float maxIdleSpeed = 1.15f;
    [Tooltip("Intervallo (s) con cui viene scelta una nuova velocità idle.")]
    public float idleChangeInterval = 3f;

    [Header("Animator parameter (opzionale)")]
    [Tooltip("Se true, userà un parametro float dell'Animator invece di modificare animator.speed.")]
    public bool useAnimatorParameter = true;
    [Tooltip("Nome del parametro float nell'Animator usato per controllare la velocità idle.")]
    public string idleSpeedParameter = "IdleSpeed";

    Vector3 lastPosition;
    float smoothedSpeed = 0f;
    bool isWalking = false;

    // coroutine per gestire la randomizzazione della velocità in idle
    private Coroutine idleSpeedCoroutine;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        lastPosition = transform.position;
        smoothedSpeed = 0f;
        isWalking = false;

        // imposta valore iniziale (parametro o speed)
        if (animator != null)
        {
            if (useAnimatorParameter && HasParameter(idleSpeedParameter))
                animator.SetFloat(idleSpeedParameter, Random.Range(minIdleSpeed, maxIdleSpeed));
            else
                animator.speed = 1f;
        }

        // Se siamo già in idle all'avvio avviamo la randomizzazione
        if (!isWalking && randomizeIdleSpeed)
        {
            if (useAnimatorParameter && animator != null && !HasParameter(idleSpeedParameter))
            {
                Debug.LogWarning($"AnimationBySpeed: param '{idleSpeedParameter}' non trovato nell'Animator. Verrà usato animator.speed come fallback.");
            }
            StartIdleRandomization();
        }
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

            // quando inizia a camminare interrompiamo la randomizzazione idle e ripristiniamo velocità 1
            StopIdleRandomization();
            SetAnimatorSpeedValue(1f);
        }
        else if (!moving && isWalking)
        {
            // fine camminata -> entriamo in idle
            animator.ResetTrigger(startTrigger);
            animator.SetTrigger(stopTrigger);
            isWalking = false;

            // avvia randomizzazione della velocità se abilitata
            if (randomizeIdleSpeed)
                StartIdleRandomization();
            else
                SetAnimatorSpeedValue(1f);
        }
    }

    // imposta il valore di velocità: parametro oppure animator.speed come fallback
    void SetAnimatorSpeedValue(float value)
    {
        if (animator == null) return;

        if (useAnimatorParameter && HasParameter(idleSpeedParameter))
            animator.SetFloat(idleSpeedParameter, value);
        else
            animator.speed = value;
    }

    // avvia la coroutine che randomizza la velocità dell'animator durante l'idle
    void StartIdleRandomization()
    {
        if (animator == null || idleSpeedCoroutine != null) return;
        idleSpeedCoroutine = StartCoroutine(IdleRandomSpeedCoroutine());
    }

    // ferma la coroutine e ripristina velocità a 1
    void StopIdleRandomization()
    {
        if (idleSpeedCoroutine != null)
        {
            StopCoroutine(idleSpeedCoroutine);
            idleSpeedCoroutine = null;
        }
        SetAnimatorSpeedValue(1f);
    }

    System.Collections.IEnumerator IdleRandomSpeedCoroutine()
    {
        if (animator == null) yield break;

        while (true)
        {
            float rnd = Random.Range(minIdleSpeed, maxIdleSpeed);
            SetAnimatorSpeedValue(rnd);
            yield return new WaitForSeconds(Mathf.Max(0.01f, idleChangeInterval));
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

        if (randomizeIdleSpeed)
            StartIdleRandomization();
        else
            SetAnimatorSpeedValue(1f);
    }

    public void ForceWalking()
    {
        if (animator == null) return;
        animator.ResetTrigger(stopTrigger);
        animator.SetTrigger(startTrigger);
        isWalking = true;

        StopIdleRandomization();
        SetAnimatorSpeedValue(1f);
    }

    void OnDisable()
    {
        StopIdleRandomization();
    }

    // helper per verificare presenza parametro nell'Animator
    bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        var ps = animator.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].name == paramName && ps[i].type == AnimatorControllerParameterType.Float)
                return true;
        }
        return false;
    }
}
