using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class MultilayerNeuralNetwork : MonoBehaviour
{
    private NeuralNetwork network;
    public AdvancedDraw draw;
    public NeuralNetworkColorer colorer;
    public bool sigmoid = false;
   

    private void Awake()
    {
        network = new NeuralNetwork(new[] { 2, 3, 2 }, sigmoid);
    }

    public void SetWeight(int layer, int x, int y, double value)
    {
        network.layers[layer].weights[x, y] = value;
    }

    public void SetBias(int layer, int x, double value)
    {
        network.layers[layer].biases[x] = value;
    }

    public int CalculateOutput(double[] values)
    {
        return network.GetMaxOutput(values);
    }

    public void UpdateVisuals()
    {
        draw.UpdateTexture();
        var weights = network.GetWeights();
        for (int i = 0; i < weights.Count; i++)
        {
            colorer.SetWeightColor(i, (float) weights[i]);
        }
        
        var bias = network.GetNodes();
        for (int i = 0; i < bias.Count; i++)
        {
            colorer.SetNodeColor(i, (float) bias[i]);
        }
    }

    public double DataPointCost(double guess, double expected)
    {
        //Debug.Log(guess.ToString("F6") + " "  + expected);
        var semival =  (expected - guess);
        return semival * semival;
    }

    public double Cost(CafeData data)
    {
        double[] outputs = network.CalculateOutputs(draw.NormalizeInput(data));
        double[] expected = data.ExpectedOuput();
        double cost = 0;
        for (int i = 0; i < outputs.Length; i++)
        {
            cost += DataPointCost(outputs[i], expected[i]);
        }

        return cost;
    }

    public double CalculateCosts(DataSetCafe set)
    {
        double totalCost = 0;
        for (int i = 0; i < set.data.Count; i++)
        {
            var cost = Cost(set.data[i]);
            totalCost += cost;
    
        }
        return totalCost/set.data.Count;
    }

    public int Correct(CafeData data)
    {
        int output = network.GetMaxOutput(draw.NormalizeInput(data));
        int expected = data.valid ? 0 : 1;

        return output == expected ? 1 : 0;
    }
    
    public int CalculateCorrects(DataSetCafe set)
    {
        int totalCorrect = 0;
        for (int i = 0; i < set.data.Count; i++)
        {
            totalCorrect += Correct(set.data[i]);
        }

        return totalCorrect;
    }
}

public class NeuralNetwork
{
    public Layer[] layers;
    public bool sigmoid = false;
    public NeuralNetwork(int[] layerSizes, bool sigmoid)
    {
        layers = new Layer[layerSizes.Length - 1];
        for (int i = 0; i < layerSizes.Length -1; i++)
        {
            layers[i] = new Layer(layerSizes[i], layerSizes[i  + 1], sigmoid);
        }
        this.sigmoid = sigmoid;
    }

    public double[] CalculateOutputs(double[] inputs)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            inputs = layers[i].CalculateOutputs(inputs);
        }
        return inputs;
    }

    public int GetMaxOutput(double[] input)
    {
        double[] outputs = CalculateOutputs(input);
        int maxIndex = 0;

        for (int i = 1; i < outputs.Length; i++)
        {
            if (outputs[i] > outputs[maxIndex])
                maxIndex = i;
        }

        return maxIndex;
    }

    public List<double> GetNodes()
    {
        List<double> nodes = new List<double>();
        for (int i = 0; i < layers.Length; i++)
        {
            for (int j = 0; j < layers[i].biases.Length; j++)
            {
                nodes.Add(layers[i].biases[j]);
            }
            
        }

        return nodes;
    }
    
    public List<double> GetWeights()
    {
        List<double> weigths = new List<double>();
        for (int i = 0; i < layers.Length; i++)
        {
            for (int j = 0; j < layers[i].weights.GetLength(0); j++)
            {
                for (int k = 0; k < layers[i].weights.GetLength(1); k++)
                {
                    weigths.Add(layers[i].weights[j, k]);
                }
               
            }
            
        }

        return weigths;
    }
}
public class Layer
{
    public int lengthIn, lengthOut;
    public double[,] weights;
    public double[] biases;
    public bool sigmoid;
    
    public Layer(int numIn,int numOut, bool sigmoid)
    {
        this.sigmoid = sigmoid;
        lengthIn = numIn;
        lengthOut = numOut;
        weights = new double[numIn, numOut];
        biases = new double[numOut];
    }

    public double[] CalculateOutputs(double[] inputs)
    {
        double[] woghtedResults = WeightResult(weights, inputs, biases);
        return ActivateNeurons(woghtedResults);
    }

    public double[] ActivateNeurons(double[] weighted)
    {
        if (sigmoid)
        {
            for (int i = 0; i < weighted.Length; i++)
            {
                weighted[i] = SigmoidActivation(weighted[i]);
            }
        }
        else
        {
            for (int i = 0; i < weighted.Length; i++)
            {
                weighted[i] = StepActivation(weighted[i]);
            }
        }

        return weighted;
    }
    public double StepActivation(double weightedInput)
    {
        return weightedInput > 0 ? 1 : 0;
    }
    
    public static double SigmoidActivation(double value)
    {
        return 1 / (1 + Math.Exp(-value));
    }

    
    public  double[] WeightResult(double[,] w, double[] x, double[] b)
    {
        if (w.GetLength(0) != x.GetLength(0))
        {
            Debug.LogError("Matrix dimension not match: " + w.GetLength(1) + ", " + x.GetLength(0));
            return null;
        }
        if (w.GetLength(1) != b.GetLength(0))
        {
            Debug.LogError("Matrix dimension not match: " + w.GetLength(0) + ", " + b.GetLength(0));
            return null;
        }
        double[] result = new double[w.GetLength(1)];
        for (int j = 0; j < w.GetLength(1); j++)
        {
            double value = 0;
            for (int i = 0; i < w.GetLength(0); i++)
            {
                value += w[i, j] * x[i];
            }

            result[j] = value + b[j];
        }

        return result;
    }
}

