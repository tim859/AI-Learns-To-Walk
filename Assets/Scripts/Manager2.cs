using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager2 : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    private bool isTraining = false;
    private int populationSize = 50;
    private int generationNumber = 0;
    private int[] layers;
    private List<NeuralNetwork> nets;
    private List<Player> playerList = null;

    void Timer()
    {
        isTraining = false;
    }

    private void Update()
    {
        if (!isTraining)
        {
            if (generationNumber == 0)
            {
                InitPlayerNeuralNetworks();
            }
            else
            {
                nets.Sort();

                for (int i = 0; i < populationSize / 2; i++)
                {
                    nets[i] = new NeuralNetwork(nets[i + (populationSize / 2)]);
                    nets[i].Mutate();

                    nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i + (populationSize / 2)]);
                }

                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].SetFitness(0f);
                }
            }

            generationNumber++;

            isTraining = true;
            Invoke("Timer", 15f);
            CreatePlayerBodies();
        }
    }

    void InitPlayerNeuralNetworks()
    {
        if (playerList != null)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                GameObject.Destroy(playerList[i].gameObject);
            }
        }

        playerList = new List<Player>();

        for (int i = 0; i < populationSize; i++)
        {
            Player player = ((GameObject)Instantiate(playerPrefab, ))
        }
    }

    void CreatePlayerBodies()
    {

    }

    private void CreateBoomerangBodies()
    {
        if (boomerangList != null)
        {
            for (int i = 0; i < boomerangList.Count; i++)
            {
                GameObject.Destroy(boomerangList[i].gameObject);
            }

        }

        boomerangList = new List<Boomerang>();

        for (int i = 0; i < populationSize; i++)
        {
            Boomerang boomer = ((GameObject)Instantiate(boomerPrefab, new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), 0), boomerPrefab.transform.rotation)).GetComponent<Boomerang>();
            boomer.Init(nets[i], hex.transform);
            boomerangList.Add(boomer);
        }

    }

    void InitBoomerangNeuralNetworks()
    {
        //population must be even, just setting it to 20 incase it's not
        if (populationSize % 2 != 0)
        {
            populationSize = 20;
        }

        nets = new List<NeuralNetwork>();


        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate();
            nets.Add(net);
        }
    }
}
