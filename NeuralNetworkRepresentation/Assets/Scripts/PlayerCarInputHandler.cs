using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarInputHandler : MonoBehaviour
{

    private Car car;
    void Start()
    {
        car = GetComponent<Car>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = Vector2.zero;
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        car.SetInputVector(input);
    }
}
