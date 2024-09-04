using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarsUIManager : MonoBehaviour
{
    private GeneticManager mgr;
    public float timeScale = 1;

    [Header("UI Components")]
    public TextMeshProUGUI timeScaleText;
    public TextMeshProUGUI currentBatch, currentGeneration, bestFitness, stopedCars, popSize, batchSize, batchTime;
    private int _batchSize = 0, _maxBatchs;
    private int _lastBatch = 1, _lastGeneration = 1, _lastCars = 0;
    void Start()
    {
        mgr = FindObjectOfType<GeneticManager>();
        Time.timeScale = timeScale;
        popSize.text = $"Population size: {mgr.GetPopSize()}";
        batchSize.text = $"Batch size: {mgr.GetBatchSize()}";
        _batchSize = mgr.GetBatchSize();
        _maxBatchs = mgr.GetMaxBatches();
        batchTime.text = $"Batch time: {mgr.GetBatchTime()} seconds";
        stopedCars.text = $"Cars stopped: {GeneticManager.CarsStopped}/{_batchSize}";
        currentBatch.text = $"Current batch: 1/{_maxBatchs}";
    }

    // Update is called once per frame
    void Update()
    {
        
        if(_lastBatch != mgr.GetCurrentBatch() + 1)
        {
            _lastBatch = mgr.GetCurrentBatch() + 1;
            currentBatch.text = $"Current batch: {_lastBatch}/{_maxBatchs}";
        }
        if(_lastGeneration != mgr.GetGeneration())
        {
            _lastGeneration = mgr.GetGeneration();
            currentGeneration.text = $"Current generation: {_lastGeneration}";
            bestFitness.text = $"Best fitness in last generation: {mgr.bestFitness.ToString("N2")}";
        }
        if(_lastCars != GeneticManager.CarsStopped)
        {
            _lastCars = GeneticManager.CarsStopped;
            stopedCars.text = $"Cars stopped: {_lastCars}/{_batchSize}";
        }
    }

    public void SetTimeScale(float timeScale)
    {
        timeScaleText.text = $"Time scale: {timeScale.ToString("N1")}";
        this.timeScale = timeScale;
        Time.timeScale = timeScale;
    }
}
