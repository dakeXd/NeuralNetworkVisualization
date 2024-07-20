using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWeights : MonoBehaviour
{
    public MultilayerNeuralNetwork network;
    public List<Slider> sliders;
    public int range = 25;
    public bool canUpdateVisuals = false;
    private IEnumerator Start()
    {
        yield return null;
        foreach (var slider in sliders)
        {
            if(!network.draw.learning)
                slider.value = Random.Range(-range, range);
        }
        network.UpdateVisuals();
        canUpdateVisuals = true;
    }

    public void SetW_I1_1(float val)
    {
        network.SetWeight(0, 0, 0, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_I1_2(float val)
    {
        network.SetWeight(0, 0, 1, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_I1_3(float val)
    {
        network.SetWeight(0, 0, 2, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_I2_1(float val)
    {
        network.SetWeight(0, 1, 0, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_I2_2(float val)
    {
        network.SetWeight(0, 1, 1, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_I2_3(float val)
    {
        network.SetWeight(0, 1, 2, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetB_M1(float val)
    {
        network.SetBias(0, 0, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    
    public void SetB_M2(float val)
    {
        network.SetBias(0, 1, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    
    public void SetB_M3(float val)
    {
        network.SetBias(0, 2, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    
    public void SetW_M1_1(float val)
    {
        network.SetWeight(1, 0, 0, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_M1_2(float val)
    {
        network.SetWeight(1, 0, 1, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_M2_1(float val)
    {
        network.SetWeight(1, 1, 0, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_M2_2(float val)
    {
        network.SetWeight(1, 1, 1, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_M3_1(float val)
    {
        network.SetWeight(1, 2, 0, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetW_M3_2(float val)
    {
        network.SetWeight(1, 2, 1, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    public void SetB_O1(float val)
    {
        network.SetBias(1, 0, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    
    public void SetB_O2(float val)
    {
        network.SetBias(1, 1, val);
        if(canUpdateVisuals)
            network.UpdateVisuals();
    }
    
    
    
}
