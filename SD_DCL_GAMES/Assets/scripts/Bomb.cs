using UnityEngine;

public class Bomb : MonoBehaviour
{

    Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Launch()
    {
        
    }

    public void Explode()
    {
        Debug.Log("BOOM!");
    }
}
