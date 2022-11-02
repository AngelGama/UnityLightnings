using System.Collections.Generic;
using UnityEngine;

public class LightningBehaviour : MonoBehaviour
{
    // Bolts and branchs prefabs
    public GameObject BoltPrefab;
    public GameObject BranchPrefab;

    // Transparency
    public float alpha = 1.5f;

    // Speed at which bolts will fade out
    public float fadeOutRate = 0.03f;
    [Range(0.5f, 2f)]

    // Lightning frequency variables
    public float timer = 0.5f;
    float lastCheck = 0f;

    // For pooling
    List<GameObject> activeBoltsObj;
    List<GameObject> inactiveBoltsObj;
    int maxBolts = 1000;

    // Different lightning modes
    enum Mode : byte
    {
        bolt,
        branch,
        moving,
        nodes,
        burst
    }

    // Default mode is branch
    Mode currentMode = Mode.branch;

    // Contains all of the pieces for the moving bolt
    List<GameObject> movingBolt = new List<GameObject>();

    // Moving bolt variables
    Vector2 lightningEnd = new Vector2(50, 50);
    Vector2 lightningVelocity = new Vector2(1, 0);

    // Contains all of the pieces for the branches
    List<GameObject> branchesObj;
    public Vector2 pos1, pos2;

    void Start()
    {
        // Initialize lists
        activeBoltsObj = new List<GameObject>();
        inactiveBoltsObj = new List<GameObject>();
        branchesObj = new List<GameObject>();

        // Grab the parent we'll be assigning to our bolt pool
        GameObject pool = GameObject.Find("LightningPoolHolder");

        for (int i = 0; i < maxBolts; i++)
        {
            // Create bolt from prefab
            GameObject bolt = (GameObject)Instantiate(BoltPrefab);

            // Assign parent
            bolt.transform.parent = pool.transform;

            // Initialize lightning with a preset number of max sexments
            bolt.GetComponent<LightningBolt>().Initialize(50);

            // Set inactive at start
            bolt.SetActive(false);

            // Store in the inactive list
            inactiveBoltsObj.Add(bolt);
        }
    }

    void Update()
    {
        GameObject boltObj;
        LightningBolt boltComponent;

        int activeLineCount = activeBoltsObj.Count;

        // Loop through active lines
        for (int i = activeLineCount - 1; i >= 0; i--)
        {
            // Pull GameObject
            boltObj = activeBoltsObj[i];

            // Get the LightningBolt component
            boltComponent = boltObj.GetComponent<LightningBolt>();

            // If the bolt has faded out
            if (boltComponent.isComplete)
            {
                // Deactive the segments it contains
                boltComponent.DeactivateSegments();

                // Set it inactive
                boltObj.SetActive(false);

                // Move it to the inactive list
                activeBoltsObj.RemoveAt(i);
                inactiveBoltsObj.Add(boltObj);
            }
        }

        // Generates a new lightning bolt every 0.5-2 seconds
        if ((Time.time - lastCheck) >= timer)
        {
            lastCheck = Time.time;

            // Randomly initialize lightning variables
            timer = Random.Range(0.5f, 2f);
            fadeOutRate = Random.Range(0.002f, 0.007f);
            currentMode = (Mode)Random.Range(0, 5);
            alpha = Random.Range(0.9f, 1.9f);

            pos1 = new Vector2(Random.Range(-20, 20), 11f);
            pos2 = new Vector2(Random.Range(-15, 15), -13f);

            // Handle the current mode appropriately
            switch (currentMode)
            {
                case Mode.bolt:

                    // Create a (pooled) bolt from pos1 to pos2
                    CreatePooledBolt(pos1, pos2, Color.white, 1f);
                    break;

                case Mode.branch:

                    // Instantiate from branch prefab
                    GameObject branchObj = (GameObject)GameObject.Instantiate(BranchPrefab);

                    // Get the branch component
                    LightningBranch branchComponent = branchObj.GetComponent<LightningBranch>();

                    // Initialize the branch component using position data
                    branchComponent.Initialize(pos1, pos2, BoltPrefab, alpha, fadeOutRate);

                    // Add it to active branches list 
                    branchesObj.Add(branchObj);
                    break;

                case Mode.moving:

                    // Prevent from getting a 0 magnitude
                    if (Vector2.Distance(pos1, pos2) <= 0)
                    {
                        // Sets a random position
                        Vector2 adjust = Random.insideUnitCircle;

                        // Failsafe
                        if (adjust.magnitude <= 0) adjust.x += .1f;

                        // Adjust the end position
                        pos2 += adjust;
                    }

                    // Clear out any old moving bolt
                    for (int i = movingBolt.Count - 1; i >= 0; i--)
                    {
                        Destroy(movingBolt[i]);
                        movingBolt.RemoveAt(i);
                    }

                    // Get the velocity so we know what direction to send the bolt in after initial creation
                    lightningVelocity = (pos2 - pos1).normalized;

                    // Instantiate from bolt prefab
                    boltObj = (GameObject)GameObject.Instantiate(BoltPrefab);

                    // Get the bolt component
                    boltComponent = boltObj.GetComponent<LightningBolt>();

                    // Initialize it with 5 max segments
                    boltComponent.Initialize(5);

                    // Activate the bolt using position data
                    boltComponent.ActivateBolt(pos1, pos2, Color.white, 1f, alpha, fadeOutRate);

                    // Add it to the list
                    movingBolt.Add(boltObj);
                    break;

                case Mode.burst:

                    // Get the difference between positions
                    Vector2 diff = pos2 - pos1;

                    // Define number of bolts 
                    int boltsInBurst = 10;

                    for (int i = 0; i < boltsInBurst; i++)
                    {
                        // Rotate around the z axis to the appropriate angle
                        Quaternion rot = Quaternion.AngleAxis((360f / boltsInBurst) * i, new Vector3(0, 0, 1));

                        // Calculate the end position for the bolt
                        Vector2 boltEnd = (Vector2)(rot * diff) + pos1;

                        // Create a (pooled) bolt from pos1 to boltEnd
                        CreatePooledBolt(pos1, boltEnd, Color.white, 1f);
                    }

                    break;
            }

        }

        // If in node mode
        if (currentMode == Mode.nodes)
        {
            // Constantly create a (pooled) bolt between the two assigned positions
            pos2.x = pos1.x + Random.Range(-1, 1);
            pos2.y = -20f;
            pos1.y = 20f;
            CreatePooledBolt(pos1, pos2, Color.white, 1f);
        }

        // Loop through any active branches
        for (int i = branchesObj.Count - 1; i >= 0; i--)
        {
            // Pull the branch lightning component
            LightningBranch branchComponent = branchesObj[i].GetComponent<LightningBranch>();

            // If it's faded out already
            if (branchComponent.isComplete)
            {
                // Destroy branch
                Destroy(branchesObj[i]);

                // Remove from the list
                branchesObj.RemoveAt(i);

                // Moves to the next branch
                continue;
            }

            // Draw and update the branch
            branchComponent.UpdateBranch();
            branchComponent.Draw();
        }

        // Loop through all of the bolts that make up the moving bolt
        for (int i = movingBolt.Count - 1; i >= 0; i--)
        {
            // Get the bolt component
            boltComponent = movingBolt[i].GetComponent<LightningBolt>();

            // If the bolt has faded out
            if (boltComponent.isComplete)
            {
                // Destroy it
                Destroy(movingBolt[i]);

                // Remove it from list
                movingBolt.RemoveAt(i);

                continue;
            }

            // Update and draw bolt
            boltComponent.UpdateBolt();
            boltComponent.Draw();
        }

        // If moving bolt is active
        if (movingBolt.Count > 0)
        {
            // Calculate where it currently ends
            lightningEnd = movingBolt[movingBolt.Count - 1].GetComponent<LightningBolt>().endPosition;

            // If the end of the bolt is within 25 units of the camera
            if (Vector2.Distance(lightningEnd, (Vector2)Camera.main.transform.position) < 25)
            {
                // Instantiate from our bolt prefab
                boltObj = (GameObject)GameObject.Instantiate(BoltPrefab);

                // Get the bolt component
                boltComponent = boltObj.GetComponent<LightningBolt>();

                // Initialize it with a maximum of 5 segments
                boltComponent.Initialize(5);

                // Activate the bolt using position data
                boltComponent.ActivateBolt(lightningEnd, lightningEnd + lightningVelocity, Color.white, 1f, alpha, fadeOutRate);

                // Add it to the list
                movingBolt.Add(boltObj);

                // Update and draw a new bolt
                boltComponent.UpdateBolt();
                boltComponent.Draw();
            }
        }

        // Update and draw active bolts
        for (int i = 0; i < activeBoltsObj.Count; i++)
        {
            activeBoltsObj[i].GetComponent<LightningBolt>().UpdateBolt();
            activeBoltsObj[i].GetComponent<LightningBolt>().Draw();
        }
    }

    // Calculate distance squared
    public float DistanceSquared(Vector2 a, Vector2 b)
    {
        return ((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
    }

    void CreatePooledBolt(Vector2 source, Vector2 dest, Color color, float thickness)
    {
        // If there is an inactive bolt to pull from the pool
        if (inactiveBoltsObj.Count > 0)
        {
            // Pull the GameObject
            GameObject boltObj = inactiveBoltsObj[inactiveBoltsObj.Count - 1];

            // Set it active
            boltObj.SetActive(true);

            // Move it to the active list
            activeBoltsObj.Add(boltObj);
            inactiveBoltsObj.RemoveAt(inactiveBoltsObj.Count - 1);

            // Get the bolt component
            LightningBolt boltComponent = boltObj.GetComponent<LightningBolt>();

            // Activate the bolt using the given position data
            boltComponent.ActivateBolt(source, dest, color, thickness, alpha, fadeOutRate);
        }
    }
}
