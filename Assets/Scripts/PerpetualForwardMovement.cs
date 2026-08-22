using UnityEngine;

public class PerpetualForwardMovement : MonoBehaviour
{

    [SerializeField] public float forwardForce = 5.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * forwardForce * Time.deltaTime);
    }
}
