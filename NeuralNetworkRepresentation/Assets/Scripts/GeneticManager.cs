using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GeneticNetwork;

public class GeneticManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int poblationSize = 30;
    [SerializeField] private int iterationTime = 30;
    [Range(0f,1f)]
    [SerializeField] private float parentScale = 0.92f;
    [Range(0f, 1f)]
    [SerializeField] private float unbiasedMutationProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float biasedMutationProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float nodeMutationProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float crossoverWeightsProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float crossoverNodesProb = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float mutationProb = 0.2f;
    [SerializeField] private int nodesMutationAmount = 2;
    [Header("Components")]
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private Transform raceStart;
    private int iteration;

    public static int CarsStopped = 0;

    private List<CarNeuralNetwork> cars;
    private List<GeneticNetwork> parentNetworks;

    private void Awake()
    {
      
        cars = new List<CarNeuralNetwork>();
        parentNetworks = new List<GeneticNetwork>();
    }
    private void Start()
    {
        FirstGeneration();
    }
    private void Update()
    {
        if(CarsStopped >= poblationSize)
        {
            EndEarly();
        }
    }
    public void FirstGeneration()
    {
        CarsStopped = 0;
        iteration = 1;
        for (int i = 0; i < poblationSize; i++)
        {
            //Debug.Log(raceStart.localPosition);
            var newCar = Instantiate(carPrefab, raceStart.position, raceStart.rotation, raceStart.parent);
            newCar.name = "Car_" + i;
            cars.Add(newCar.GetComponent<CarNeuralNetwork>());
        }
        StartCoroutine(MaxTime());
    }

    public GeneticOperation GetRandomGeneticOperation()
    {
        float operationAdd = unbiasedMutationProb + biasedMutationProb + nodeMutationProb + crossoverNodesProb + crossoverWeightsProb;
        float selection = Random.Range(0, operationAdd);
        if (selection < unbiasedMutationProb)
            return GeneticOperation.ImpartialMutation;
        if (selection < unbiasedMutationProb + biasedMutationProb)
            return GeneticOperation.PartialMutation;
        if (selection < unbiasedMutationProb + biasedMutationProb + nodeMutationProb)
            return GeneticOperation.NodeMutation;
        if (selection < unbiasedMutationProb + biasedMutationProb + nodeMutationProb + crossoverWeightsProb)
            return GeneticOperation.WeightCrossover;
        return GeneticOperation.NodeCrossover;
    }
    public GeneticNetwork GetOneParent()
    {
        for(int i = 0; i < parentNetworks.Count; i++)
        {
            float p = Random.Range(0f, 1f);
            if (p < parentScale)
            {
                //Debug.Log("Parent is " + i + ", " + parentNetworks.IndexOf(parentNetworks[i]));
                return parentNetworks[i];
            }
                
        }
        return parentNetworks[0];
    }

    public GeneticNetwork[] GetTwoParent()
    {
        GeneticNetwork[] parents = new GeneticNetwork[2];
        for (int i = 0; i < parentNetworks.Count; i++)
        {
            float p = Random.Range(0f, 1f);
            if (p < parentScale)
            {
                parents[0] = parentNetworks[i];
                break;
            }
             
        }
        if (parents[0] == null)
            parents[0] = parentNetworks[0];
        for (int i = 0; i < parentNetworks.Count; i++)
        {
            float p = Random.Range(0f, 1f);
            if (p < parentScale)
            {
                if(parents[0] != parentNetworks[i])
                {
                    parents[1] = parentNetworks[i];
                    break;
                }
            }    
        }
        if (parents[1] == null)
            parents[1] = parents[0] == parentNetworks[0] ? parentNetworks[1] : parentNetworks[0];
        //Debug.Log("Parent 1: "+ parentNetworks.IndexOf(parents[0]) + ", " + "Parent 2: " + parentNetworks.IndexOf(parents[1]));
        return parents;
    }
    public void NextGeneration()
    {
        CarsStopped = 0;
        for (int i = 0; i < poblationSize; i++)
        {
            GeneticOperation operation = i == 0 ? GeneticOperation.None : GetRandomGeneticOperation();
            var newCar = Instantiate(carPrefab, raceStart.position, raceStart.rotation, raceStart.parent);
            newCar.name = "Car_" + i;

            var carNet = newCar.GetComponent<CarNeuralNetwork>();
            //Debug.Log(operation);
            carNet.network.NoMutation(parentNetworks[0]);
            GeneticNetwork[] parents = GetTwoParent();
            switch (operation)
            {
                case GeneticOperation.None:
                    carNet.network.NoMutation(parentNetworks[0]);
                    break;
                case GeneticOperation.PartialMutation:
                    carNet.network.PartialMutation(GetOneParent(), mutationProb);
                    break;
                case GeneticOperation.ImpartialMutation:
                    carNet.network.ImpartialMutation(GetOneParent(), mutationProb);
                    break;
                case GeneticOperation.WeightCrossover:
                    carNet.network.WeightCrossover(parents[0], parents[1]);
                    break;
                case GeneticOperation.NodeCrossover:
                    carNet.network.NodeCrossover(parents[0], parents[1]);
                    break;
                case GeneticOperation.NodeMutation:
                    carNet.network.MutateNodes(GetOneParent(), nodesMutationAmount);
                    break;
            }
            cars.Add(carNet);
        }
        parentNetworks.Clear();
        StartCoroutine(MaxTime());
    }

    public IEnumerator MaxTime()
    {
        yield return new WaitForSeconds(iterationTime);
        EndGeneration();
    }

    public void EndEarly()
    {
        StopAllCoroutines();
        EndGeneration();
    }
    public void EndGeneration()
    {
        CarsStopped = 0;
        cars.Sort((a, b) => a.score.CalculateScore().CompareTo(b.score.CalculateScore()));
        Debug.Log("Iteration " + iteration + ": Best car is " + cars[cars.Count-1] + ": " + cars[cars.Count - 1].score.CalculateScore());
   
        for (int i = cars.Count-1; i >= 0; i--)
        {
            //Debug.Log(i + " " +cars[i].gameObject.name + ": " + cars[i].score.CalculateScore());
            parentNetworks.Add(cars[i].network);
            Destroy(cars[i].gameObject);
            cars.RemoveAt(i);
        }
        cars.Clear();
        iteration++;
        Invoke(nameof(NextGeneration), 1);

    }



}
