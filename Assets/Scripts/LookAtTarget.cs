using UnityEngine;


public class LookAtTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float startTime=0;
    [SerializeField] float endTime=3;
    [SerializeField] float turnSpeed = 3;
    private bool active;

    private float timer;



    private void Start()
    {

    }
    public void StartLoockAt()
    {
        active = true;
    }
    public void EndLoockAt()
    {
        active = false;
        enabled = false;
    }


    void Update()
    {
        if (!active) return;



        if (target)
        {
            timer += Time.deltaTime;
            if (timer >= startTime)
            {
                var targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }


            if (endTime > 0)
            {
                if (timer > endTime)
                    EndLoockAt();
            }
        }
    }
}
