using System.Collections.Generic;
using UnityEngine;

public class LightningBolt : MonoBehaviour
{
    // List of all of the active/inactive lines
    public List<GameObject> activeLineObj;
    public List<GameObject> inactiveLineObj;

    // Line Prefab
    public GameObject linePrefab;

    // Transparency
    public float alpha;

    // Speed at which bolts will fade out
    public float fadeOutRate;

    // Bolt color
    public Color tint;

    // Start bolt position
    public Vector2 startPosition { get { return activeLineObj[0].GetComponent<Line>().startPoint; } }

    // End bolt position
    public Vector2 endPosition { get { return activeLineObj[activeLineObj.Count - 1].GetComponent<Line>().endPoint; } }

    // True if the bolt has completely faded out
    public bool isComplete { get { return alpha <= 0; } }

    public void Initialize(int maxSegments)
    {
        // Initialize lists for pooling
        activeLineObj = new List<GameObject>();
        inactiveLineObj = new List<GameObject>();

        for (int i = 0; i < maxSegments; i++)
        {
            // Instantiate from Line Prefab
            GameObject line = (GameObject)GameObject.Instantiate(linePrefab);

            // Parent line to bolt object
            line.transform.parent = transform;

            // Set line inactive
            line.SetActive(false);

            // Add line to the list
            inactiveLineObj.Add(line);
        }
    }

    public void ActivateBolt(Vector2 source, Vector2 dest, Color color, float thickness, float alphaBolt, float fadeOutRateBolt)
    {
        // Store tint
        tint = color;

        // Store transparency
        alpha = alphaBolt;

        // Store fade out rate
        fadeOutRate = fadeOutRateBolt;

        // Prevent from getting a 0 magnitude
        if (Vector2.Distance(dest, source) <= 0)
        {
            Vector2 adjust = Random.insideUnitCircle;
            if (adjust.magnitude <= 0) adjust.x += 0.1f;
            dest += adjust;
        }

        // Difference from source to destination
        Vector2 slope = dest - source;
        Vector2 normal = (new Vector2(slope.y, -slope.x)).normalized;

        // Distance between source and destination
        float distance = slope.magnitude;

        List<float> positions = new List<float>();
        positions.Add(0);

        for (int i = 0; i < distance / 4; i++)
        {
            // Generate random positions between 0 and 1 to break up the bolt
            positions.Add(Random.Range(0.25f, 0.75f));
        }

        positions.Sort();

        const float Sway = 80;
        const float Jaggedness = 1 / Sway;

        // Affects how wide the bolt is allowed to spread
        float spread = 1f;

        // Start at the source
        Vector2 prevPoint = source;

        // No previous displacement, so just 0
        float prevDisplacement = 0;

        for (int i = 1; i < positions.Count; i++)
        {
            // Don't allow more than is in the pool
            int inactiveCount = inactiveLineObj.Count;
            if (inactiveCount <= 0) break;

            float pos = positions[i];

            // Prevent sharp angles by ensuring very close positions also have small perpendicular variation
            float scale = (distance * Jaggedness) * (pos - positions[i - 1]);

            // Points near the middle of the bolt can be further from the central line.
            float envelope = pos > 0.95f ? 20 * (1 - pos) : spread;

            float displacement = Random.Range(-Sway, Sway);
            displacement -= (displacement - prevDisplacement) * (1 - scale);
            displacement *= envelope;

            // Calculate the end point
            Vector2 point = source + (pos * slope) + (displacement * normal);

            activateLine(prevPoint, point, thickness);
            prevPoint = point;
            prevDisplacement = displacement;
        }

        activateLine(prevPoint, dest, thickness);
    }

    public void DeactivateSegments()
    {
        for (int i = activeLineObj.Count - 1; i >= 0; i--)
        {
            GameObject line = activeLineObj[i];
            line.SetActive(false);
            activeLineObj.RemoveAt(i);
            inactiveLineObj.Add(line);
        }
    }

    void activateLine(Vector2 A, Vector2 B, float thickness)
    {
        int inactiveCount = inactiveLineObj.Count;

        // Only activate if it can pull from inactive
        if (inactiveCount <= 0) return;

        // Pull the GameObject
        GameObject line = inactiveLineObj[inactiveCount - 1];

        // Set it active
        line.SetActive(true);

        // Get the Line component
        Line lineComponent = line.GetComponent<Line>();
        lineComponent.SetColor(Color.white);
        lineComponent.startPoint = A;
        lineComponent.endPoint = B;
        lineComponent.thickness = thickness;
        inactiveLineObj.RemoveAt(inactiveCount - 1);
        activeLineObj.Add(line);
    }

    public void Draw()
    {
        // If the bolt has faded out, no need to draw
        if (alpha <= 0) return;

        foreach (GameObject obj in activeLineObj)
        {
            Line lineComponent = obj.GetComponent<Line>();
            lineComponent.SetColor(tint * (alpha * 0.6f));
            lineComponent.Draw();
        }
    }

    public void UpdateBolt()
    {
        alpha -= fadeOutRate;
    }

    // Returns the point where the bolt is at a given fraction of the way through the bolt
    // 0: return the start of the bolt
    // 1: return the end.
    public Vector2 GetPoint(float position)
    {
        Vector2 start = startPosition;
        float length = Vector2.Distance(start, endPosition);
        Vector2 dir = (endPosition - start) / length;
        position *= length;

        // Find the appropriate line
        Line line = activeLineObj.Find(x => Vector2.Dot(x.GetComponent<Line>().endPoint - start, dir) >= position).GetComponent<Line>();
        float lineStartPos = Vector2.Dot(line.startPoint - start, dir);
        float lineEndPos = Vector2.Dot(line.endPoint - start, dir);
        float linePos = (position - lineStartPos) / (lineEndPos - lineStartPos);

        return Vector2.Lerp(line.startPoint, line.endPoint, linePos);
    }

}
