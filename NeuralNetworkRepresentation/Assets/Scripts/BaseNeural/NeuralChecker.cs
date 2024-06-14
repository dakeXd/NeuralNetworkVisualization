using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralChecker : MonoBehaviour
{
    public double w1_1, w1_2, w2_1, w2_2;

    public NeuralNetworkColorer colorer;
    public double bias1, bias2;
    public Draw Draw;
    public bool useActivation = false;
    public bool sigmoid = false;
    public bool calculateValue(float input1, float input2)
    {
        var o1 = (input1 * w1_1 + input2* w2_1) + bias1;
        var o2 = (input1 * w1_2 + input2* w2_2) + bias2;
        if (useActivation)
        {
            if (sigmoid)
            {
                //Debug.Log(o1 + ", " + o2 + ": " + Sigmoid(o1) + ", "+ Sigmoid(o2));
                return Sigmoid(o1) > 0.5f;
           
            }
            
          
            return activation(o1) > activation(o2);
            
        }
     
        return o1 > o2;
        
        
    }

    public double activation(double weightedInput)
    {
        return weightedInput > 0 ? 1 : 0;
    }
    public static double Sigmoid(double value)
    {
        return 1 / (1 + Math.Exp(-value));
    }
    
    public void SetW1_1(float v)
    {
        w1_1 = v;
        Draw.UpdateTexture();
        colorer.SetWeightColor(0, v);
    }
    public void SetW1_2(float v)
    {
        w1_2 = v;
        Draw.UpdateTexture();
        colorer.SetWeightColor(1, v);
    }
    public void SetW2_1(float v)
    {
        w2_1 = v;
        Draw.UpdateTexture();
        colorer.SetWeightColor(2, v);
    }
    public void SetW2_2(float v)
    {
        w2_2 = v;
        Draw.UpdateTexture();
        colorer.SetWeightColor(3, v);
    }
    public void SetB1(float v)
    {
        bias1 = v;
        Draw.UpdateTexture();
        colorer.SetNodeColor(0, v);
    }
    public void SetB2(float v)
    {
        bias2 = v;
        Draw.UpdateTexture();
        colorer.SetNodeColor(1, v);
    }
}
