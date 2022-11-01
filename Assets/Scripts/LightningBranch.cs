using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBranch : MonoBehaviour
{
    // Bolts in the branch
    List<GameObject> boltsObj = new List<GameObject>();

    // If there are no bolts, then the branch is complete
    public bool isComplete { get { return boltsObj.Count == 0; } }

    // Start branch position
    public Vector2 startBranch { get; private set; }

    // End branch position
    public Vector2 endBranch { get; private set; }

    public void Initialize(Vector2 start, Vector2 end, GameObject boltPrefab, float alpha, float fadeOutRate)
    {
        // Store start and end positions
        startBranch = start;
        endBranch = end;

        // Create the  main bolt
        GameObject mainBoltObj = (GameObject)GameObject.Instantiate(boltPrefab);

        // Get the LightningBolt component
        LightningBolt mainBoltComponent = mainBoltObj.GetComponent<LightningBolt>();

        // Initialize the bolt with a max of 5 segments
        mainBoltComponent.Initialize(5);

        // Activate the bolt with the position data
        mainBoltComponent.ActivateBolt(start, end, Color.white, 1f, alpha, fadeOutRate);

        // Add main bolt to the list
        boltsObj.Add(mainBoltObj);

        // Randomly determine how many sub branches there will be (3 - 8)
        int numBranches = Random.Range(3, 8);

        // Calculate the difference between start and end points
        Vector2 diff = end - start;

        // Pick a bunch of random points between 0 and 1 and sort them
        List<float> branchPoints = new List<float>();
        for (int i = 0; i < numBranches; i++) branchPoints.Add(Random.value);
        branchPoints.Sort();

        // Go through those points
        for (int i = 0; i < branchPoints.Count; i++)
        {
            Vector2 boltStart = mainBoltComponent.GetPoint(branchPoints[i]);

            // Get rotation of 30 degrees. Alternate between rotating left and right.
            Quaternion rot = Quaternion.AngleAxis(30 * ((i & 1) == 0 ? 1 : -1), new Vector3(0, 0, 1));

            // Calculate how much to adjust for the end position
            Vector2 adjust = rot * (Random.Range(0.5f, 0.75f) * diff * (1 - branchPoints[i]));

            // Get the end position
            Vector2 boltEnd = adjust + boltStart;

            // Instantiate from bolt prefab
            GameObject boltObj = (GameObject)GameObject.Instantiate(boltPrefab);

            // Get the LightningBolt component
            LightningBolt boltComponent = boltObj.GetComponent<LightningBolt>();

            // Initialize the bolt with a max of 5 segments
            boltComponent.Initialize(5);

            // Activate the bolt with the position data
            boltComponent.ActivateBolt(boltStart, boltEnd, Color.white, 1f, alpha, fadeOutRate);

            // Add bolt to the list
            boltsObj.Add(boltObj);
        }
    }

    public void UpdateBranch()
    {
        // Go through active bolts
        for (int i = boltsObj.Count - 1; i >= 0; i--)
        {
            // Get the GameObject
            GameObject boltObj = boltsObj[i];

            // Get the LightningBolt component
            LightningBolt boltComp = boltObj.GetComponent<LightningBolt>();

            // Update or fade out the bolt
            boltComp.UpdateBolt();

            // If the bolt has faded
            if (boltComp.isComplete)
            {
                // Remove it from list
                boltsObj.RemoveAt(i);

                // Destroy it
                Destroy(boltObj);
            }
        }
    }

    // Draw active bolts on screen
    public void Draw()
    {
        foreach (GameObject boltObj in boltsObj)
        {
            boltObj.GetComponent<LightningBolt>().Draw();
        }
    }
}
