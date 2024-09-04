using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GeneticNetwork;
using Random = UnityEngine.Random;

public class GeneticManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int maxBatches = 3;
    [SerializeField] private int iterationTime = 30, batchSize = 100;
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
    private int[] layers = new int[] { 6, 5, 2 };
    private int iteration, batch = 0, poblationSize;

    public static int CarsStopped = 0;

    [NonSerialized] public float bestFitness = 0;

    private List<CarNeuralNetwork> cars;
    private List<GeneticNetwork> parentNetworks;

    private void Awake()
    {
        poblationSize = batchSize * maxBatches;
        cars = new List<CarNeuralNetwork>(batchSize);
        parentNetworks = new List<GeneticNetwork>(poblationSize);
    }
    private void Start()
    {
        FirstGeneration();
    }
    private void Update()
    {
        if(CarsStopped >= batchSize)
        {
            EndEarly();
        }
    }

   
    public void FirstGeneration()
    {
        CarsStopped = 0;
        iteration = 1;
        batch = 0;
        for (int i = 0; i < poblationSize; i++)
        {
            var net = new GeneticNetwork(layers, Activation.Sigmoid, Activation.Tanh);
            foreach (var layer in net.layers)
            {
                layer.InitRandomWeights(false);
            }
            parentNetworks.Add(net);
        }
        for (int i = 0; i < batchSize; i++)
        {
            //Debug.Log(raceStart.localPosition);
            var newCar = Instantiate(carPrefab, raceStart.position, raceStart.rotation, raceStart.parent);
            newCar.name = "Car_" + i;
            var n = newCar.GetComponent<CarNeuralNetwork>();
            n.network = parentNetworks[i];
            cars.Add(n);

          
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

    public GeneticOperation GetRandomMutateOperation()
    {
        float operationAdd = unbiasedMutationProb + biasedMutationProb + nodeMutationProb;
        float selection = Random.Range(0, operationAdd);
        if (selection < unbiasedMutationProb)
            return GeneticOperation.ImpartialMutation;
        if (selection < unbiasedMutationProb + biasedMutationProb)
            return GeneticOperation.PartialMutation;
        return GeneticOperation.NodeMutation;
      
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
        BreedNextGeneration();
        for (int i = 0; i < cars.Count; i++)
        {
            //GeneticOperation operation = i == 0 ? GeneticOperation.None : GetRandomGeneticOperation();
            //var newCar = Instantiate(carPrefab, raceStart.position, raceStart.rotation, raceStart.parent);
            //newCar.name = "Car_" + i;

            var carNet = cars[i];
            carNet.transform.position = raceStart.position;
            carNet.transform.rotation = raceStart.rotation;

            //Debug.Log(operation);
            //carNet.network.NoMutation(parentNetworks[0]);
            /*
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
            }*/
            
            cars[i].network = parentNetworks[i];
            cars[i].ResetCar();
        }

        batch = 0;
        StartCoroutine(MaxTime());
    }

    public void StartNextBatch()
    {

        for (int i = 0; i < cars.Count; i++)
        {

            var carNet = cars[i];
            carNet.transform.position = raceStart.position;
            carNet.transform.rotation = raceStart.rotation;
            cars[i].ResetCar();
        }

        StartCoroutine(MaxTime());
    }

    public IEnumerator MaxTime()
    {
        yield return new WaitForSeconds(iterationTime);
        NextBatch();
    }

    public void EndEarly()
    {
        StopAllCoroutines();
        NextBatch();
    }

    public void NextBatch()
    {
        int preBatchOffset = batch * batchSize;
        batch++;
        CarsStopped = 0;

        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].network.fitness = cars[i].score.CalculateScore();
            parentNetworks[preBatchOffset + i] = cars[i].network;
        }
        if(batch >= maxBatches)
        {
            EndGeneration();
        }
        else
        {
            int batchOffset = batch * batchSize;
            for (int i = 0; i < cars.Count; i++)
            {
                cars[i].network = parentNetworks[batchOffset + i];
            }
            StartNextBatch();
        }
    }
    public void EndGeneration()
    {
        //Debug.Log("Cars stopped: " + CarsStopped);
        CarsStopped = 0;
        parentNetworks.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        Debug.Log("Iteration " + iteration + ": Best individue is " + parentNetworks[0] + ": " + parentNetworks[0].fitness);
        bestFitness = parentNetworks[0].fitness;
        iteration++;
        NextGeneration();

    }

    public void Crossover(ref double[] p1,ref double[] p2)
    {
        double[] ch1 = new  double[p1.Length];
        double[] ch2 = new double[p1.Length];

        for(int i = 0; i < p1.Length; i++)
        {
            bool swap = Random.Range(0f, 1f) < 0.5f;
            ch1[i] = swap ? p2[i] : p1[i];
            ch2[i] = swap ? p1[i] : p2[i];
        }
        p1 = ch1;
        p2 = ch2;
        
    }

    public GeneticNetwork[] Breed(GeneticNetwork mother, GeneticNetwork father)
    {
        int[] layers = new int[] { 6, 5,  2 };
        GeneticNetwork child1 = new GeneticNetwork(layers, Activation.Sigmoid, Activation.Tanh);
        GeneticNetwork child2 = new GeneticNetwork(layers, Activation.Sigmoid, Activation.Tanh);

        double[] motherChromosome = mother.Encode();
        double[] fatherChromosome = father.Encode();

        Crossover(ref motherChromosome, ref fatherChromosome);

        child1.Decode(motherChromosome);
        child2.Decode(fatherChromosome);

        return new GeneticNetwork[] { child1, child2 };
    }

    public void Mutate(GeneticNetwork creature, GeneticOperation op)
    {
        switch (op)
        {
            case GeneticOperation.None:
                creature.NoMutation(parentNetworks[0]);
                break;
            case GeneticOperation.PartialMutation:
                creature.PartialMutation(creature, mutationProb);
                break;
            case GeneticOperation.ImpartialMutation:
                creature.ImpartialMutation(creature, mutationProb);
                break;
            case GeneticOperation.WeightCrossover:
                break;
            case GeneticOperation.NodeCrossover:
                break;
            case GeneticOperation.NodeMutation:
                creature.MutateNodes(creature, nodesMutationAmount);
                break;
        }

    }

    // Creates next generation through Roulette wheel selection
    public void BreedNextGeneration()
    {
        // Create a new generation 
        var nextGeneration = new List<GeneticNetwork>();
      
        //Save the two best cars for next gen
        nextGeneration.Add(parentNetworks[0]);
        nextGeneration.Add(parentNetworks[1]);

        // Create 2 children for every breeding of NN
        for (int i = 0; i < parentNetworks.Count / 2; i++)
        {
            // Select parents to breed
            var parents = GetTwoParent();


            //Breed the two selected parents and add them to the next generation
            //Debug.Log("Breeding: " + parent1Index + " with fitness " + population[parent1Index].fitness);
            //Debug.Log("and " + parent2Index + " with fitness " + population[parent2Index].fitness);

            GeneticNetwork[] children = Breed(parents[0], parents[1]);

            // Mutate children
            Mutate(children[0], GetRandomMutateOperation());
            Mutate(children[1], GetRandomMutateOperation());

            // Add the children to the next generation
            nextGeneration.Add(children[0]);
            nextGeneration.Add(children[1]);
        } //  End foor loop -- Breeding

        // Make the children adults
        for (int i = 1; i < parentNetworks.Count; i++)
        {
            parentNetworks[i] = nextGeneration[i];
        }
    }// End NextGeneration()

    //Getters
    public int GetPopSize()
    {
        return poblationSize;
    }

    public int GetBatchSize()
    {
        return batchSize;
    }

    public int GetBatchTime()
    {
        return iterationTime;
    }

    public int GetMaxBatches()
    {
        return maxBatches;
    }

    public int GetGeneration()
    {
        return iteration;
    }

    public int GetCurrentBatch()
    {
        return batch;
    }
}