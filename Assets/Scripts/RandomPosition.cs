using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPosition : MonoBehaviour
{
    public List<Transform> points=new List<Transform>();
    private Transform point;
    public float timeMove = 30;
    float timer;

    void Start()
    {
        if(points.Count==0)
        points.Add(transform);
        MoveTo();
    }

    [ContextMenu("MoveTo")]
    public void MoveTo()
    {
        point = points[Random.Range(0, points.Count)];
        transform.position = point.position;
        transform.rotation = point.rotation;
    }

    private void Update()
    {
        if (timeMove > 0)
        {
            timer += Time.deltaTime;
            if(timer> timeMove)
            {
                MoveTo();
                timer = 0;
            }
        }
    }
}
