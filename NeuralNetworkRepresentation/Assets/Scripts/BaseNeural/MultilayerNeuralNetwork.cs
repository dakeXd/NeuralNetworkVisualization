using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MultilayerNeuralNetwork : MonoBehaviour
{
    private NeuralNetwork network;
    public AdvancedDraw draw;
    public NeuralNetworkColorer colorer;
    public bool sigmoid = false;
    public float learningRate = 0.2f;
    public TextMeshProUGUI learningRateText;
    public bool batchExamples = false;
    public int batchingSize = 20;
    public bool backPropagation = false;
    private int lastBatch = 0;
    public void SetLearningRate(float learningRate)
    {
        this.learningRate = learningRate;
        PlayerPrefs.SetFloat("LearnignRate", learningRate);
        if (learningRateText != null)
        {
            learningRateText.text = "Valor de aprendizaje: " + learningRate.ToString("F2");
            learningRateText.transform.parent.GetComponentInChildren<Slider>().SetValueWithoutNotify(learningRate);
        }
            
    }

    private void Awake()
    {
        network = new NeuralNetwork(new[] { 2, 3, 2 }, sigmoid, backPropagation);
        learningRate = PlayerPrefs.GetFloat("LearnignRate", learningRate);
        SetLearningRate(learningRate);
        if (draw.learning)
        {
            foreach (var layer in network.layers)
            {
                layer.InitRandomWeights();
            }
        }
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

    public void UpdateVisuals(bool updateText = true)
    {
        if(updateText)
            draw.UpdateTextureFull();
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

    public double CalculateCosts(DataSetCafe set)
    {
        double totalCost = 0;
        for (int i = 0; i < set.data.Count; i++)
        {
            var cost = network.Cost(set.data[i]);
            totalCost += cost;
    
        }
        return totalCost/set.data.Count;
    }

    public int Correct(CafeData data)
    {
        int output = network.GetMaxOutput(data.Input());
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

    public void Learn(DataSetCafe dataSet)
    {
        List<CafeData> cafe = new List<CafeData>(batchingSize);
        for (int i = 0; i < batchingSize; i++)
        {
            lastBatch++;
            if (lastBatch >= dataSet.data.Count)
                lastBatch = 0;
            cafe.Add(dataSet.data[lastBatch]);
        }

        var set = batchExamples ? cafe : dataSet.data;
        foreach (var c in set)
        {
            network.Learn(c, learningRate);
        }
        UpdateVisuals(false);
      
    }
}

public class NeuralNetwork
{
    public Layer[] layers;
    public bool sigmoid = false, backpropagation = false;
    public const float H = 0.0001f;
    public NeuralNetwork(int[] layerSizes, bool sigmoid, bool backpropagation)
    {
        layers = new Layer[layerSizes.Length - 1];
        for (int i = 0; i < layerSizes.Length -1; i++)
        {
            layers[i] = new Layer(layerSizes[i], layerSizes[i  + 1], sigmoid, backpropagation);
        }
        this.sigmoid = sigmoid;
        this.backpropagation = backpropagation;
    }

    public double Cost(CafeData data)
    {
        double[] outputs = CalculateOutputs(data.Input());
        double[] expected = data.ExpectedOuput();
        double cost = 0;
        for (int i = 0; i < outputs.Length; i++)
        {
            cost += DataPointCost(outputs[i], expected[i]);
        }

        return cost;
    }
    
    public double DataPointCost(double guess, double expected)
    {
        //Debug.Log(guess.ToString("F6") + " "  + expected);
        var semival =  (expected - guess);
        return semival * semival;
    }
    
    public  double DataPointCostDerivative(double guess, double expected)
    {
        return 2 * (guess - expected);
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
    
    public void Learn(CafeData learnData, float learnRate)
    {
        if (backpropagation)
        {
            CalculateBackpropagationGradient(learnData);
        }
        else
        {
            CalculateGradient(learnData);
        }
        
        //Importante no aplciar el gradiente en el mismo loop en el que se asigna, ya que estariamos actualizando con datos alterados.
        foreach (var layer in layers)
        {
            layer.ApplyGradient(learnRate);
        }
        if(backpropagation)
            ClearAllCostGradients();
        
        
    }

    private void CalculateGradient(CafeData learnData)
    {
        double cost = Cost(learnData);
        foreach (var layer in layers)
        {
            for (int i = 0; i < layer.lengthOut; i++)
            {
                layer.biases[i] += H;
                double newCost = Cost(learnData);
                layer.biases[i] -= H;
                layer.costGradientBiases[i] = (newCost - cost) / H;

                for (int j = 0; j < layer.lengthIn; j++)
                {
                    layer.weights[j, i] += H;
                    double newCostW = Cost(learnData);
                    layer.weights[j, i] -= H;
                    layer.costGradientWeights[j, i] = (newCostW - cost) / H;
                }
            }
        }
    }

    private void CalculateBackpropagationGradient(CafeData learnData)
    {
        //actualizar todos los valores de la red
        CalculateOutputs(learnData.Input());
        //Pesos en la ultima capa
        //
        //                                   L     L
        //                                 dz     da    dC
        //      L-1      L       L           j     j 
        //     a * sig'(dz) * 2(a - y)^2 = ___ * ___ * ___  
        //      j        j      j   j        L-1    L     L
        //                                 dw     dz    da
        //                                   jk    j     j
        Layer output = layers[layers.Length - 1];
        double[] neuronValues = output.CalculateOutpuNeuronDerivativeValues(learnData.ExpectedOuput());
        output.UpdateGradientsBackP(neuronValues);

        for (int hiddenLayer = layers.Length - 2; hiddenLayer >= 0; hiddenLayer--)
        {
            Layer l = layers[hiddenLayer];
            neuronValues = l.CalculateHiddenLayerNeuronDerivativeValues(neuronValues, layers[hiddenLayer+1]);
            l.UpdateGradientsBackP(neuronValues);
        }
        
        
    }
    public void ClearAllCostGradients()
    {
        foreach (var layer in layers)
        {
            layer.ClearGradients();
        }
    }
   
}
public class Layer
{
    //layer normal info
    public int lengthIn, lengthOut;
    public double[,] weights;
    public double[] biases;
    //Learning layer data
    public double[,] costGradientWeights;
    public double[] costGradientBiases;
    private double[] weightedInputs;
    private double[] nodeActOutputs;
    private double[] inputs;
    private bool sigmoid, backpropagation;
    public const float H = 0.0001f;
    public Layer(int numIn,int numOut, bool sigmoid, bool backpropagation)
    {
        this.sigmoid = sigmoid;
        this.backpropagation = backpropagation;
        lengthIn = numIn;
        lengthOut = numOut;
        inputs = new double[numIn];
        weights = new double[numIn, numOut];
        costGradientWeights = new double[numIn, numOut];
        biases = new double[numOut];
        costGradientBiases = new double[numOut];
        nodeActOutputs = new double[numOut];
        weightedInputs = new double[numOut];
    }

    public void ApplyGradient(float learnRate)
    {
        for (int i = 0; i < lengthOut; i++)
        {
            for (int j = 0; j < lengthIn; j++)
            {
                weights[j, i] -= costGradientWeights[j, i] * learnRate;
            }
            biases[i] -= costGradientBiases[i] * learnRate;
        }
    }
    public void InitRandomWeights()
    {
        for (int i = 0; i < weights.GetLength(0); i++)
        {
            for (int j = 0; j < weights.GetLength(1); j++)
            {
                float value = Random.Range(-1f, 1f);
                value = value / Mathf.Sqrt(lengthIn);
                weights[i, j] = value;
            }
        }
    }
    public double[] CalculateOutputs(double[] inputsIn)
    {
        this.inputs = inputsIn;
        weightedInputs = WeightResult(weights, inputs, biases);
        ActivateNeurons(weightedInputs);
        return nodeActOutputs;
    }

    public double[] ActivateNeurons(double[] weighted)
    {
        if (sigmoid)
        {
            for (int i = 0; i < weighted.Length; i++)
            {
                nodeActOutputs[i] = SigmoidActivation(weighted[i]);
            }
        }
        else
        {
            for (int i = 0; i < weighted.Length; i++)
            {
                nodeActOutputs[i] = StepActivation(weighted[i]);
            }
        }

        return nodeActOutputs;
    }
    
    public double StepActivation(double weightedInput)
    {
        return weightedInput > 0 ? 1 : 0;
    }
    
    public static double SigmoidActivation(double value)
    {
        return 1 / (1 + Math.Exp(-value));
    }

    public static double SigmoidActivationDerivative(double value)
    {
        var activation = SigmoidActivation(value);
        return activation * (1 - activation);
        //return SigmoidActivation(1 - activation);
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
    
    public void ClearGradients(){
        for (int j = 0; j < weights.GetLength(1); j++)
        {
            double value = 0;
            for (int i = 0; i < weights.GetLength(0); i++)
            {
                costGradientWeights[i, j] = 0;
            }

            costGradientBiases[j] = 0;
        }   
    }
    public  double DataPointCostDerivative(double guess, double expected)
    {
        return 2 * (guess - expected);
    }
    public double[] CalculateOutpuNeuronDerivativeValues(double[] expectedOut)
    {
        double[] neuronValues = new double[expectedOut.Length];
        for (int i = 0; i < expectedOut.Length; i++)
        {
            //                          L
        //                            da    dC
        //          L       L           j 
        // o= sig'(dz) * 2(a - y)^2 = ___ * ___ 
        //          j      j   j        L     L
        //                            dz    da
        //                              j     j
        neuronValues[i] = SigmoidActivationDerivative(weightedInputs[i]) *
                          DataPointCostDerivative(nodeActOutputs[i], expectedOut[i]);
        }

        return neuronValues;
    }
    
    
    public void UpdateGradientsBackP(double[] neuronValues)
    {
        //           L
        //   dC    dz
        //           j    L   L+1         L+n    L-1  L   L+1         L+n
        //  ___ = ___ * [h * h *   ... * o]  =  a * [h * h *   ... * o]
        //     L     L                           j
        //   cw    dw
        //     jk    jk
        // Para lso bias se cambio w por 1
        for (int out_ = 0; out_ < lengthOut ; out_++)
        {
            for (int in_ = 0; in_ < lengthIn ; in_++)
            {
                //El coste de gradiente es el sumatorio de todas las varianzas causadas por el peso inicial
                costGradientWeights[in_, out_] += inputs[in_] * neuronValues[out_];
            }

            costGradientBiases[out_] += neuronValues[out_]; //*1
        }
    }
    
    public double[] CalculateHiddenLayerNeuronDerivativeValues(double[] nextLayerValues, Layer nextLayer)
    {
        double[] neuronValues = new double[lengthOut];
        for (int originNeuron = 0; originNeuron < lengthOut; originNeuron++)
        {
            double newNeuronValue = 0;
            for (int objetiveNeuron = 0; objetiveNeuron < nextLayerValues.Length; objetiveNeuron++)
            {
                //    L-1    L-1
                //   dz    da
                //    j      j        L-1        L-1
                //  ___ * ___  * p  = w  *  sig'(z) = h
                //    L-2    L-1      jk         j
                //   da    dz
                //    jk     j
                //Debug.Log("[In: " + lengthIn + ", Out: " + lengthOut + "] origin " + originNeuron + ", objetive " + objetiveNeuron);
                //Debug.Log("NextLayer weights [" + nextLayer.weights.GetLength(0) + "x" + nextLayer.weights.GetLength(1) +"], nextLayerValues: " + nextLayerValues.Length);
                newNeuronValue += (nextLayer.weights[originNeuron, objetiveNeuron] * nextLayerValues[objetiveNeuron]);
            }

            newNeuronValue *= SigmoidActivationDerivative(weightedInputs[originNeuron]);
            neuronValues[originNeuron] = newNeuronValue;
        }
        
        return neuronValues;
    }
}

