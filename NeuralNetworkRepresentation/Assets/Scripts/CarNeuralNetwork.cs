using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarNeuralNetwork : MonoBehaviour
{
    private Car car;
    [NonSerialized] public GeneticNetwork network;
    private CarSensors sensors;
    [NonSerialized] public CheckpointHelper score;
    bool hit = false;
    
    void Awake()
    {
       
        sensors = GetComponent<CarSensors>();
        score = GetComponent<CheckpointHelper>();
        car = GetComponent<Car>();
       
        hit = false;
    }

    public void ResetCar()
    {
        hit = false;
        score.ResetCar();
        car.RestartRotation();
        car.rb.isKinematic = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (!hit)
                GeneticManager.CarsStopped++;
            hit= true;
            car.rb.velocity = Vector2.zero;
            car.rb.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        if (hit)
        {
            Vector2 inp = Vector2.zero;
            car.SetInputVector(inp);
            return;
        }
        double[] output = network.CalculateOutputs(sensors.SensorsAsInputs());
        Vector2 inputVector = new Vector2((float)output[0], (float)output[1]);
        car.SetInputVector(inputVector);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
    }
}
