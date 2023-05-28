using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerState 
{
    Frozen,
    Walking,
    Cloud
}

public class PlayerMotor : MonoBehaviour
{
    [SerializeField]
    Camera cam;

    //Information for setting up the freezing during animations
    [SerializeField]
    List<string> animations = new List<string>();
    [SerializeField]
    List<int> animationtimes = new List<int>();
    //I really wish you could get dictionaries to work in inspector
    Dictionary<string, int> animLookup = new Dictionary<string, int>();

    public string Animating = "";
    int animationFrames;

    [SerializeField]
    Transform prefablightning;
    [SerializeField]
    Transform prefabRain;
    [SerializeField]
    Transform prefabFire;

    Transform fusedCloud;
    public Transform Checkpoint;

    //in seconds, how long until you can click again
    [SerializeField]
    float boltCooldown, rainCooldown;
    float boltCountdown, rainCountdown;

    [SerializeField]
    float speed;
    [SerializeField]
    float cloudspeed;
    [SerializeField]
    float fallrate;
    public PlayerState mode;

    public float radius;
    public float cloudradius;

    public float walkheight, cloudheight;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < (int)Mathf.Min(animations.Count, animationtimes.Count); i++)
        {
            animLookup.Add(animations[i], animationtimes[i]);
        }
    }

    private void Update()
    {
        // tick down animation frames, end animation
        if(mode == PlayerState.Frozen && animations.Contains(Animating)) 
        {
            if (animationFrames > 0)
                animationFrames--;
            else
            {
                animationFrames = 0;
                Animating = "";
                mode = PlayerState.Walking;
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 lastPosition = transform.position;
        Vector3 newposition = transform.position;
        RaycastHit hit;
        switch (mode)
        {
            case PlayerState.Frozen:

                break;
            case PlayerState.Walking:

                newposition += new Vector3(Input.GetAxis("Horizontal"), walkheight, Input.GetAxis("Vertical")) * speed * Time.fixedDeltaTime;
                Physics.Linecast(lastPosition, newposition, out hit);
                //move position to 1 radius away from where it hits a collider if there's a collider.
                if (hit.collider != null)
                    newposition = hit.point + Vector3.Normalize(lastPosition - newposition) * radius;
                Physics.Linecast(transform.up, fallrate * -transform.up, out hit);
                //Conform to slopes, falls if further than fallrate.
                if (hit.collider != null && hit.distance < 1 + fallrate)
                    walkheight = hit.point.y;
                else
                    walkheight = lastPosition.y - (fallrate * Time.deltaTime);

                newposition.y = walkheight + radius;
                break;
            case PlayerState.Cloud:
                newposition += new Vector3(Input.GetAxis("Horizontal"), cloudheight, Input.GetAxis("Vertical")) * cloudspeed * Time.fixedDeltaTime;
                Physics.Linecast(lastPosition, newposition, out hit);
                //move position to 1 radius away from where it hits a collider if there's a collider.
                if (hit.collider != null)
                    newposition = hit.point + Vector3.Normalize(lastPosition - newposition) * cloudradius;
                newposition.y = cloudheight;
                break;
        }

        transform.position = newposition;

        if (boltCountdown > 0)
            boltCountdown -= Time.fixedDeltaTime;
        if (rainCountdown > 0)
            rainCountdown -= Time.fixedDeltaTime;

        if (Input.GetButton("Left Click"))
            ClickInteract();
    }


    public void BecomeCloud(Transform cloud) 
    {
        //set up graphical effect
        Transform bolt = Instantiate(prefablightning, transform.position, transform.rotation) as Transform;
        bolt.GetComponent<LightningPath>().RunPath(transform.position, cloud.position);

        //set flags to take over cloud
        transform.position = cloud.position;
        cloud.transform.parent = this.transform;
        cloud.GetComponent<PathObject>().enabled = false;
        if (fusedCloud.GetComponent<Destructable>() != null)
            fusedCloud.GetComponent<Destructable>().enabled = false;
        mode = PlayerState.Cloud;
        fusedCloud = cloud;

        //always ready to rain when just joining a cloud, block lightninging yourself out of the cloud for a bit
        rainCountdown = 0;
        boltCountdown = boltCooldown;
    }

    //Return to checkpoint
    public void LeaveCloud()
    {
        //set up graphical effect
        Transform bolt = Instantiate(prefablightning, transform.position, transform.rotation) as Transform;
        bolt.GetComponent<LightningPath>().RunPath(transform.position, Checkpoint.position);

        //set up fire ring around where you return
        Transform fire = Instantiate(prefabFire, Checkpoint.position, Checkpoint.rotation) as Transform;


        //set flags to leave cloud and drop onto checkpoint
        fusedCloud.parent = null;
        //make sure to reenable an scripts on the cloud 
        fusedCloud.GetComponent<PathObject>().enabled = true;
        if(fusedCloud.GetComponent<Destructable>() != null) 
            fusedCloud.GetComponent<Destructable>().enabled = true;
        transform.position = Checkpoint.position;
        mode = PlayerState.Cloud;


    }
    //Land where you click
    public void LeaveCloud(Vector3 target)
    {
        //set up graphical effect
        Transform bolt = Instantiate(prefablightning, transform.position, transform.rotation) as Transform;
        bolt.GetComponent<LightningPath>().RunPath(transform.position, target);

        //set up fire ring around where you return
        Transform fire = Instantiate(prefabFire, target, Quaternion.identity) as Transform;


        //set flags to leave cloud and drop onto checkpoint
        fusedCloud.parent = null;
        //make sure to reenable an scripts on the cloud 
        fusedCloud.GetComponent<PathObject>().enabled = true;
        if (fusedCloud.GetComponent<Destructable>() != null)
            fusedCloud.GetComponent<Destructable>().enabled = true;
        transform.position = target;
        mode = PlayerState.Cloud;


    }

    public bool StartAnimation(string anim) 
    {
        if (animations.Contains(anim)) 
        {
            Animating = anim;
            animationFrames = animLookup[anim];
            return true;
        }
        return false;
    }

    public void ClickInteract() 
    {
        if (boltCountdown > 0)
            return;

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        //enter and leave clouds
        if(Physics.Raycast(ray, out hit, 100)) 
        {
            if (mode == PlayerState.Walking && hit.transform.GetComponent<PathObject>() && hit.transform.GetComponent<PathObject>().mode == pathType.cloud)
                BecomeCloud(hit.transform);
            else if (mode == PlayerState.Cloud && hit.transform.tag == "walkable")
                LeaveCloud(hit.point);
        }

    }
}
