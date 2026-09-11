using UnityEngine;
using System.Collections;

public class RandomizeAnimation : MonoBehaviour
{
    [Tooltip("Max trigger value (inclusive). Random will pick 0..max_TriggerNumbers")]
    public int max_TriggerNumbers = 0;

    [Tooltip("Nome del parametro int nell'Animator che controlla le transizioni")]
    public string animatorParameterName = "Randomize";

    [Header("Intervallo random (s) tra i cambi di stato")]
    public float minInterval = 2f;
    public float maxInterval = 6f;

    [Tooltip("Se true, dopo aver impostato il trigger verrà riportato a 0 dopo resetDelay secondi")]
    public bool resetToZero = false;
    public float resetDelay = 0.5f;

    private int randomTriggerNumber;
    private Animator animator;
    private Coroutine randomRoutine;
    private Coroutine resetCoroutine;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("RandomizeAnimation: Animator mancante sul GameObject.");
        }

        // manteniamo il comportamento esistente: random speed all'avvio
        if (animator != null)
            animator.speed = Random.Range(0.4f, 1f);

        // avvia la coroutine solo se hai trigger validi (>0) e l'animator ha il parametro int richiesto
        if (max_TriggerNumbers > 0)
        {
            if (animator == null)
            {
                Debug.LogWarning("RandomizeAnimation: non posso avviare la randomizzazione perché manca Animator.");
            }
            else if (!HasIntParameter(animator, animatorParameterName))
            {
                Debug.LogWarning($"RandomizeAnimation: Animator non contiene un parametro int chiamato '{animatorParameterName}'. Randomizzazione disabilitata.");
            }
            else
            {
                randomRoutine = StartCoroutine(RandomizeLoop());
            }
        }
    }

    private IEnumerator RandomizeLoop()
    {
        // breve attesa iniziale casuale per evitare sincronizzazioni
        yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            if (animator == null) yield break;

            int next = Random.Range(0, max_TriggerNumbers + 1);

            // applica solo se cambia (evita scritture inutili)
            if (next != randomTriggerNumber)
            {
                randomTriggerNumber = next;
                animator.SetInteger(animatorParameterName, randomTriggerNumber);

                if (resetToZero)
                {
                    // ferma precedente reset e avvia il nuovo
                    if (resetCoroutine != null)
                    {
                        StopCoroutine(resetCoroutine);
                        resetCoroutine = null;
                    }
                    if (resetDelay > 0f)
                        resetCoroutine = StartCoroutine(ResetParameterAfterDelay(resetDelay));
                }
            }
        }
    }

    private IEnumerator ResetParameterAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
        {
            animator.SetInteger(animatorParameterName, 0);
            randomTriggerNumber = 0;
        }
        resetCoroutine = null;
    }

    private void OnDisable()
    {
        if (randomRoutine != null)
        {
            StopCoroutine(randomRoutine);
            randomRoutine = null;
        }
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
    }

    // helper: verifica esistenza di parametro int nell'Animator
    bool HasIntParameter(Animator anim, string paramName)
    {
        if (anim == null || string.IsNullOrEmpty(paramName)) return false;
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].name == paramName && ps[i].type == AnimatorControllerParameterType.Int)
                return true;
        }
        return false;
    }
}