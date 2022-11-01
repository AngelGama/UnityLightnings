using UnityEngine;

public class Line : MonoBehaviour
{
    public Vector2 startPoint;
    public Vector2 endPoint;

    public float thickness;

    public GameObject startCapChild, lineChild, endCapChild;


    // Create a new line
    public Line(Vector2 a, Vector2 b, float Thickness)
    {
        startPoint = a;
        endPoint = b;
        thickness = Thickness;
    }

    // Set the color of the line
    public void SetColor(Color color)
    {
        startCapChild.GetComponent<SpriteRenderer>().color = color;
        lineChild.GetComponent<SpriteRenderer>().color = color;
        endCapChild.GetComponent<SpriteRenderer>().color = color;
    }

    // Draw the line
    public void Draw()
    {
        Vector2 difference = endPoint - startPoint;
        float rotation = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        // Set the scale of the line to reflect length and thickness
        lineChild.transform.localScale = new Vector3(100 * (difference.magnitude / lineChild.GetComponent<SpriteRenderer>().sprite.rect.width),
                                                     thickness,
                                                     lineChild.transform.localScale.z
                                                     );

        startCapChild.transform.localScale = new Vector3(startCapChild.transform.localScale.x,
                                                         thickness,
                                                         startCapChild.transform.localScale.z
                                                         );

        endCapChild.transform.localScale = new Vector3(endCapChild.transform.localScale.x,
                                                       thickness,
                                                       endCapChild.transform.localScale.z
                                                       );

        // Rotate the line so that it's facing the right direction
        lineChild.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        startCapChild.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        endCapChild.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation + 180));

        // Move the line to be centered on the starting point
        lineChild.transform.position = new Vector3(startPoint.x, startPoint.y, lineChild.transform.position.z);
        startCapChild.transform.position = new Vector3(startPoint.x, startPoint.y, startCapChild.transform.position.z);
        endCapChild.transform.position = new Vector3(startPoint.x, startPoint.y, endCapChild.transform.position.z);

        // Rotation convertion from degrees to radians to use Cos/Sin functions
        rotation *= Mathf.Deg2Rad;

        // Store values so we only have to access once
        float lineChildWorldAdjust = lineChild.transform.localScale.x * lineChild.GetComponent<SpriteRenderer>().sprite.rect.width / 2f;
        float startCapChildWorldAdjust = startCapChild.transform.localScale.x * startCapChild.GetComponent<SpriteRenderer>().sprite.rect.width / 2f;
        float endCapChildWorldAdjust = endCapChild.transform.localScale.x * endCapChild.GetComponent<SpriteRenderer>().sprite.rect.width / 2f;

        // Adjust the middle segment to the appropriate position
        lineChild.transform.position += new Vector3(0.01f * Mathf.Cos(rotation) * lineChildWorldAdjust,
                                                     0.01f * Mathf.Sin(rotation) * lineChildWorldAdjust,
                                                     0f
                                                     );

        //Adjust the start cap to the appropriate position
        startCapChild.transform.position -= new Vector3(0.01f * Mathf.Cos(rotation) * startCapChildWorldAdjust,
                                                         0.01f * Mathf.Sin(rotation) * startCapChildWorldAdjust,
                                                         0f
                                                         );

        //Adjust the end cap to the appropriate position
        endCapChild.transform.position += new Vector3(0.01f * Mathf.Cos(rotation) * lineChildWorldAdjust * 2,
                                                       0.01f * Mathf.Sin(rotation) * lineChildWorldAdjust * 2,
                                                       0f
                                                       );
        endCapChild.transform.position += new Vector3(0.01f * Mathf.Cos(rotation) * endCapChildWorldAdjust,
                                                       0.01f * Mathf.Sin(rotation) * endCapChildWorldAdjust,
                                                       0f
                                                       );
    }
}
