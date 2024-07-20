using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkColorer : MonoBehaviour
{
    public List<SpriteRenderer> weights;
    public List<SpriteRenderer> nodes;
    public Color positiveColor;
    public Color negativeColor;
    public int maxRange;
    
    public void SetWeightColor(int index, float weight)
    {
        Color c = weight > 0 ? positiveColor : negativeColor;
        Debug.Log(weight);
        if (weight == 0)
        {
            c.a = 0;
        }
        else
        {
            c.a = (Math.Abs(weight)) / maxRange;
        }

        weights[index].color = c;
    }
    
    public void SetNodeColor(int index, float nodeBias)
    {
        Color c = nodeBias > 0 ? positiveColor : negativeColor;
       
        if (nodeBias == 0)
        {
            c = Color.white;
        }
        else
        {
            c = Color.Lerp(c, Color.white, (maxRange -Math.Abs(nodeBias)) / maxRange);
        }

        nodes[index].color = c;
    }
}
