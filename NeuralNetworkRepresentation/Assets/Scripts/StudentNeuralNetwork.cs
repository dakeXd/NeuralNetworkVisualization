using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentNeuralNetwork : MonoBehaviour
{
    private NeuralNetwork network;
    public int batchingSize = 100;
    public float learningRate = 0.2f;
    public int[] layers;
    private int lastBatch;
    [Range(0f, 1f)]
    public float inertia = 0;

    private void Awake()
    {
        network = new NeuralNetwork(layers, Activation.Sigmoid, Activation.Softmax, true, inertia);
        lastBatch = 0;
        foreach (var layer in network.layers)
        {
            layer.InitRandomWeights();
        }
    }
    
    public bool Correct(Student student)
    {
        int output = network.GetMaxOutput(student.AsDataInput());
        int expected = (int) student.gradeclass;
        //Debug.Log(student.id + " ouptut: " + output + "expected: " + student.gradeclass);
        /*if(expected != output)
        {
            Debug.Log($"Student {student.id}, {student.gpa.ToString("N3")}: {output}|{expected}");
        }*/
        return expected == output;
        /*
        double abs = output - expected;
        abs = abs < 0 ? -abs : abs;
        return abs < 0.1;
        */
    }

    public int CalculateCorrects(Student[] set)
    {
        int totalCorrect = 0;
        for (int i = 0; i < set.Length; i++)
        {
            totalCorrect += Correct(set[i]) ? 1 : 0;
        }

        return totalCorrect;
    }

    public void Learn(Student[] dataSet)
    {
        List<Student> training = new List<Student>(batchingSize);
        for (int i = 0; i < batchingSize; i++)
        {
            lastBatch++;
            if (lastBatch >= dataSet.Length)
                lastBatch = 0;
            training.Add(dataSet[lastBatch]);
        }

        foreach (var c in training)
        {
            network.Learn(c.AsDataInput(), c.ExpectedOutput(), learningRate);
        }
    }

    public double CalculateCosts(Student[] dataSet)
    {
        double totalCost = 0;
        for (int i = 0; i <dataSet.Length; i++)
        {
            var cost = network.Cost(dataSet[i].AsDataInput(), dataSet[i].ExpectedOutput() );
            totalCost += cost;

        }
        return totalCost / dataSet.Length;
    }
}
