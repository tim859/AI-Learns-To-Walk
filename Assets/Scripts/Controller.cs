using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Controller : MonoBehaviour
{
    [Header("Neural Network Assignables")]
    [SerializeField] GameObject agentPrefab;
    [SerializeField] GameObject goal;
    [SerializeField] GameObject cam;
    [SerializeField] GameObject laser;
    [SerializeField] TMP_Text genCounter;
    [SerializeField] TMP_Text statsCounter;
    [SerializeField] TMP_Text expectedScore;
    [SerializeField] TMP_Text expectedFitness;
    [SerializeField] TMP_Text timeLeft;

    [Header("Neural Network Attributes")]
    [SerializeField] int populationSize = 50;
    [SerializeField] bool showOnlyBestAgentFromLastGen;
    [SerializeField] bool fixCameraToBestAgent;
    [SerializeField] int generationNumber = 0;
    [SerializeField] float generationTime = 15f;
    [SerializeField] float laserMaxPenalty = 0.8f;
    [SerializeField] float laserMinPenalty = 0.2f;

    private bool isTraining = false;
    private int[] layers = new int[] { 14, 10, 10, 6 };
    private List<NeuralNetwork> nets;
    private List<float> sortedNets;
    private List<Agent> agentList = null;
    int bestNet = 0;
    int deadAgents = 0;
    float currentBestScore;
    float currentBestFitness;
    float timer;

    void Timer()
    {
        isTraining = false;
        laser.GetComponent<LaserController>().ResetPosition();
        deadAgents = 0;
    }

    void Update()
    {
        timer -= Time.deltaTime; 

        if (isTraining == false)
        {
            if (generationNumber == 0)
            {
                InitAgentNeuralNetworks();
            }
            else
            {
                // find best fitness of previous generation here

                for (int i = 0; i < agentList.Count; i++)
                {
                    if (nets[i].GetFitness() > nets[bestNet].GetFitness())
                    {
                        bestNet = i;
                    }
                }

                nets.Sort();
                for (int i = 0; i < populationSize / 2; i++)
                {
                    nets[i] = new NeuralNetwork(nets[i + (populationSize / 2)]);
                    nets[i].Mutate();

                    nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i + (populationSize / 2)]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
                }

                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].SetFitness(0f);
                }
            }
            generationNumber++;
            genCounter.text = "Generation: " + generationNumber.ToString();

            isTraining = true;
            Invoke(nameof(Timer), generationTime);
            CreateAgentBodies();
            timer = generationTime;
        }

        if (showOnlyBestAgentFromLastGen)
        {
            for (int i = 0; i < agentList.Count; i++)
            {
                if (agentList[i] != null)
                {
                    if (i != bestNet)
                    {
                        Renderer[] childRenderers = agentList[i].GetComponentsInChildren<Renderer>();

                        foreach (Renderer renderer in childRenderers)
                        {
                            renderer.enabled = false;
                        }
                    }
                }
            }
            Renderer[] bestChildRenderers = agentList[bestNet].GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in bestChildRenderers)
            {
                renderer.enabled = true;
            }
        }
        else
        {
            for (int i = 0; i < agentList.Count; i++)
            {
                if (agentList[i] != null)
                {
                    Renderer[] childRenderers = agentList[i].GetComponentsInChildren<Renderer>();

                    foreach (Renderer renderer in childRenderers)
                    {
                        renderer.enabled = true;
                    }
                }
            }
        }

        if (fixCameraToBestAgent)
        {
            Vector3 newPos = new(agentList[bestNet].transform.GetChild(0).GetChild(0).position.x, cam.transform.position.y, cam.transform.position.z);

            cam.transform.position = newPos;
        }

        // check if agents have been touched by the laser
        for (int i = 0; i < agentList.Count; i++)
        {
            if (agentList[i] != null)
            {
                if (agentList[i].touchedLaser)
                {
                    // the lower that deadAgents is, the higher the multiplier is
                    //i.e. the quicker the agent dies, the more of a penalty they get
                    float diff = laserMaxPenalty - laserMinPenalty;

                    // turn the difference in between the min penalty and max penalty into n amount of different evenly spaced floats
                    // where n is the population size
                    float penalty = laserMaxPenalty - (diff * (deadAgents / populationSize));

                    // halve the fitness of the net as a penalty for touching the laser
                    // nets[i].SetFitness(nets[i].GetFitness() * penalty);

                    Destroy(agentList[i].gameObject);

                    agentList[i] = null;
                    deadAgents++;

                    if (i == bestNet)
                    {
                        CancelInvoke(nameof(Timer));
                        isTraining = false;
                        laser.GetComponent<LaserController>().ResetPosition();
                        deadAgents = 0;
                        timer = generationTime;
                    }
                }
            }
        }

        try
        {
            statsCounter.text = "Best agent is agent " + bestNet + " with a score of " + Math.Round(agentList[bestNet].GetScore(), 2) + " and a fitness of " + Math.Round(nets[bestNet].GetFitness());

            if (agentList[bestNet].GetScore() > currentBestScore)
            {
                currentBestScore = agentList[bestNet].GetScore();
            }

            if (nets[bestNet].GetFitness() > currentBestFitness)
            {
                currentBestFitness = nets[bestNet].GetFitness();
            }

            expectedScore.text = "Expected score: " + Math.Round(currentBestScore, 2);
            expectedFitness.text = "Expected fitness: " + Math.Round(currentBestFitness, 2);
            timeLeft.text = "Time left in generation: " + Math.Round(timer, 2);


            //for (int i = 0; i < nets.Count; i++)
            //{
            //    sortedNets.Add(nets[i].GetFitness());
            //}

            //sortedNets.Sort();

            //scoreboard.text = "Scoreboard\n1st place: Agent " + agentList[] + "\n2nd place: Agent " + sortedNets[1] + "\n3rd place: Agent " + sortedNets[2];
        }
        catch (NullReferenceException)
        {

        }
    }

    void CreateAgentBodies()
    {
        if (agentList != null)
        {
            for (int i = 0; i < agentList.Count; i++)
            {
                if (agentList[i] != null)
                {
                    Destroy(agentList[i].gameObject);
                }
            }
        }

        agentList = new List<Agent>();

        for (int i = 0; i < populationSize; i++)
        {
            Agent agent = Instantiate(agentPrefab, new Vector2(0.28f, -0.35f), agentPrefab.transform.rotation).GetComponent<Agent>();

            agent.Init(nets[i], goal.transform);
            agentList.Add(agent);
        }
    }

    void InitAgentNeuralNetworks()
    {
        if (populationSize % 2 != 0)
        {
            populationSize = 20;
        }

        nets = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new(layers);
            net.Mutate();
            nets.Add(net);
        }
    }
}
