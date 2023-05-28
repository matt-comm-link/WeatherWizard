using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum pathType 
{
    npc,
    cloud
}


public class PathObject : MonoBehaviour
{
    public pathType mode;

    public Vector3 pathEnd;
    public float speed;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    //move towards target
    private void FixedUpdate()
    {
        if(Vector3.Distance(transform.position, pathEnd) > speed * Time.fixedDeltaTime)
            transform.position += Vector3.Normalize(pathEnd - transform.position) * speed * Time.fixedDeltaTime;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeTarget(Vector3 newTarget)
    {
        if (Vector3.Distance(transform.position, pathEnd) > speed * Time.fixedDeltaTime)
            pathEnd = newTarget;
    }
}
