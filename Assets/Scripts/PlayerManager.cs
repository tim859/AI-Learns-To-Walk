using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject goal;
    [SerializeField] GameObject cam;

    [Header("Neural Network Attributes")]
    [SerializeField] int populationSize = 50;
    [SerializeField] bool showOnlyBestPlayer;
    [SerializeField] bool fixCameraToBestPlayer;
    [SerializeField] int generationNumber = 0;
    [SerializeField] float generationTime = 15f;

    private bool isTraining = false;
    private int[] layers = new int[] { 14, 20, 20, 6 };
    private List<NeuralNetwork> nets;
    private List<Player> playerList = null;

    void Timer()
    {
        isTraining = false;
    }

    void Update()
    {
        if (isTraining == false)
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

                    nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i + (populationSize / 2)]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
                }

                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].SetFitness(0f);
                }
            }

            generationNumber++;

            isTraining = true;
            Invoke(nameof(Timer), generationTime);
            CreatePlayerBodies();
        }

        int bestNet = 0;

        if (showOnlyBestPlayer)
        {
            float bestFitness = 0;

            for (int i = 0; i < playerList.Count; i++)
            {
                if (nets[i].GetFitness() >= bestFitness)
                {
                    bestFitness = nets[i].GetFitness();
                    bestNet = i;
                }

                Renderer[] childRenderers = playerList[i].GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in childRenderers)
                {
                    renderer.enabled = false;
                }
            }

            Renderer[] bestChildRenderers = playerList[bestNet].GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in bestChildRenderers)
            {
                renderer.enabled = true;
            }
        }
        else
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                Renderer[] childRenderers = playerList[i].GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in childRenderers)
                {
                    renderer.enabled = true;
                }
            }
        }

        if (fixCameraToBestPlayer)
        {
            Vector3 newPos = new(playerList[bestNet].transform.GetChild(0).GetChild(0).position.x, cam.transform.position.y, cam.transform.position.z);

            cam.transform.position = newPos;
        }
    }

    void CreatePlayerBodies()
    {
        if (playerList != null)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                Destroy(playerList[i].gameObject);
            }
        }

        playerList = new List<Player>();

        for (int i = 0; i < populationSize; i++)
        {
            Player player = Instantiate(playerPrefab, new Vector2(0.28f, -0.35f), playerPrefab.transform.rotation).GetComponent<Player>();

            player.Init(nets[i], goal.transform);
            playerList.Add(player);
        }

        //if (showOnlyBestPlayer & generationNumber > 1)
        //{
        //    float bestFitness = 0;
        //    int bestNet = 0;

        //    for (int i = 0; i < playerList.Count; i++)
        //    {
        //        if (nets[i].GetFitness() >= bestFitness)
        //        {
        //            bestFitness = nets[i].GetFitness();
        //            bestNet = i;
        //        }

        //        Renderer[] childRenderers = playerList[i].GetComponentsInChildren<Renderer>();

        //        foreach (Renderer renderer in childRenderers)
        //        {
        //            renderer.enabled = false;
        //        }
        //    }

        //    Renderer[] bestChildRenderers = playerList[bestNet].GetComponentsInChildren<Renderer>();

        //    foreach (Renderer renderer in bestChildRenderers)
        //    {
        //        renderer.enabled = true;
        //    }
        //}
    }

    void InitPlayerNeuralNetworks()
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
