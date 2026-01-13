using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float floatHeight = 0.3f;
    public float floatSpeed = 1.5f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.localPosition;
    }
    
    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
