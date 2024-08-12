using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarSensors : MonoBehaviour
{
    Rigidbody2D rb;
    public LayerMask hitMask;
    public float distanceScaler = 1;
    private float[] distances;
    bool debug = false;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        distances = new float[5];
        CastRays();
    }

    private void Update()
    {
        //Debug.DrawRay(transform.position, transform.up * 100, Color.red, 0f, false);
        CastRays();
    }
    void CastRays()
    {
        Vector2 direction = transform.up;
        int index = 0;
        for (int i = -90; i<= 90; i += 45)
        {
            Vector2 subDirection = Quaternion.Euler(0, 0, i) * direction;
            var hit = Physics2D.Raycast(transform.position, subDirection, Mathf.Infinity, hitMask);
            if (hit.collider != null)
            {
                Vector2 pos = new Vector2(transform.position.x, transform.position.y);
                float distance = Vector2.Distance(pos, hit.point);
                if(debug)
                    Debug.DrawRay(transform.position, subDirection * distance, Color.red, 0, false);
                distances[index] = Mathf.Clamp01(distance / distanceScaler);
            }
           
        }
    }

    public double[] SensorsAsInputs()
    {
        double[] input = new double[7];
        for (int i = 0; i < distances.Length; i++)
            input[i] = distances[i];
        Vector2 localVelocity = transform.InverseTransformDirection(rb.velocity);
        input[5] = localVelocity.x;
        input[6] = localVelocity.y;
        return input;
    }

}
