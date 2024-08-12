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
        hit = false;
        sensors = GetComponent<CarSensors>();
        score = GetComponent<CheckpointHelper>();
        car = GetComponent<Car>();
        int[] layers = new int[] {7, 10, 2};
        network = new GeneticNetwork(layers, Activation.Sigmoid, Activation.Sigmoid);
        foreach (var layer in network.layers)
        {
            layer.InitRandomWeights(false);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (!hit)
                GeneticManager.CarsStopped++;
            hit= true;
            car.rb.velocity = Vector2.zero;
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (hit)
        {
            Vector2 inp = Vector2.zero;
            car.SetInputVector(inp);
            return;
        }
        double[] output = network.CalculateOutputs(sensors.SensorsAsInputs());
        Vector2 inputVector = new Vector2((float) output[0] * 2 - 1, (float) output[1] * 2 - 1);
        car.SetInputVector(inputVector);
    }
}
