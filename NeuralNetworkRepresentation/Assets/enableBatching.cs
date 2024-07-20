using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class enableBatching : MonoBehaviour
{
    public TextMeshProUGUI text;

    public MultilayerNeuralNetwork network;

    private bool batchingActive = false;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        var savedBatch = PlayerPrefs.GetInt("Batching", 0);
        batchingActive = savedBatch == 1;
        text.text = batchingActive ? "Desactivar batching" : "Activar batching";
    }

    public void ChangeBatching()
    {
        batchingActive = !batchingActive;
        PlayerPrefs.SetInt("Batching", batchingActive ? 1 : 0);
        network.batchExamples = batchingActive;
        text.text = batchingActive ? "Desactivar batching" : "Activar batching";
    }


}
