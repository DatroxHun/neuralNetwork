using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace neuralNetwork
{
    class Program
    {
        static Random rnd = new Random();
        static int[] networkStructure = { 2, 3, 4 };
        static Network network = new Network(networkStructure);

        static float[] input = { 0, 1 };

        static void Main(string[] args)
        {
            float[] input = new float[2];
            int result = 0;
            int correct = 0;

            for (int i = 0; correct != 4 && i < 100000; i++)
            {
                correct = 0;

                for (int j = 0; j < 4; j++)
                {
                    input[0] = (j - j % 2) / 2;
                    input[1] = j % 2;

                    result = network.Push(input).First().Key;

                    if (result == (input[0] * 2 + input[1]))
                    {
                        network.Reward(result);
                        correct++;
                    }
                    else
                    {
                        network.Punish(result);
                    }
                }

                if (correct == 4)
                {
                    Console.WriteLine(i.ToString());
                    break;
                }
            }

            //Console.WriteLine(input[0].ToString());
            //Console.WriteLine(input[1].ToString());

            //Console.WriteLine();

            //Console.WriteLine("input: " + (input[0] * 2 + input[1]).ToString());
            //Console.WriteLine("output: " + result.ToString());

            //Console.WriteLine();
            //Console.WriteLine();

            while (true)
            {
                input[0] = int.Parse(Console.ReadLine());
                input[1] = int.Parse(Console.ReadLine());

                result = network.Push(input).First().Key;

                Console.WriteLine();
                // A note by Z0mzi

                Console.WriteLine("input: " + (input[0] * 2 + input[1]).ToString());
                Console.WriteLine("output: " + result.ToString());

                Console.WriteLine();
                Console.WriteLine();
            }

        }

        public class Network
        {
            public List<List<Node>> web = new List<List<Node>>();

            public Network()
            {

            }

            public Network(int[] _structure)
            {
                web.Add(new List<Node>());
                for (int i = 0; i < _structure[0]; i++)
                {
                    web[0].Add(new Node());
                }

                for (int x = 1; x < _structure.Length; x++)
                {
                    web.Add(new List<Node>());
                    for (int y = 0; y < _structure[x]; y++)
                    {
                        web[x].Add(new Node(_structure[x - 1]));
                    }
                }
            }

            public Dictionary<int, float> Push(float[] _input)
            {
                Dictionary<int, float> results = new Dictionary<int, float>();

                for (int i = 0; i < _input.Length; i++)
                {
                    web[0][i].value = _input[i];
                }

                for (int x = 1; x < web.Count; x++)
                {
                    for (int y = 0; y < web[x].Count; y++)
                    {
                        web[x][y].value = 0;
                        for (int z = 0; z < web[x][y].connections.Count; z++)
                        {
                            web[x][y].value += web[x - 1][z].value * web[x][y].connections[z];
                        }
                    }
                }

                for (int i = 0; i < web.Last().Count; i++)
                {
                    results.Add(i, web.Last()[i].value);
                }

                return results.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            public void Punish(int _punishedEndNode)
            {
                int currentPunishedNodeId = _punishedEndNode;
                int currentPunishedConnectionId;
                float punishmentAmount;

                Dictionary<int, float> connectionsId = new Dictionary<int, float>();

                for (int i = web.Count - 1; i >= 1; i--)
                {
                    connectionsId = new Dictionary<int, float>();
                    for (int j = 0; j < web[i][currentPunishedNodeId].connections.Count; j++)
                    {
                        connectionsId.Add(j, web[i][currentPunishedNodeId].connections[j]);
                    }

                    currentPunishedConnectionId = connectionsId.OrderByDescending(x => x.Value).First().Key;
                    punishmentAmount = web[i][currentPunishedNodeId].connections[currentPunishedConnectionId] * .45f;
                    web[i][currentPunishedNodeId].connections[currentPunishedConnectionId] -= punishmentAmount;

                    for (int j = 0; j < web[i][currentPunishedNodeId].connections.Count; j++)
                    {
                        if (currentPunishedConnectionId != j)
                        {
                            web[i][currentPunishedNodeId].connections[j] += punishmentAmount / (float)(web[i][currentPunishedNodeId].connections.Count - 1f);
                        }
                    }

                    currentPunishedNodeId = currentPunishedConnectionId;
                }
            }

            public void Reward(int _rewardedEndNode)
            {
                int currentRewardedNodeId = _rewardedEndNode;
                int currentRewardedConnectionId;
                float rewardAmount;

                Dictionary<int, float> connectionsId = new Dictionary<int, float>();

                for (int i = web.Count - 1; i >= 1; i--)
                {
                    connectionsId = new Dictionary<int, float>();
                    for (int j = 0; j < web[i][currentRewardedNodeId].connections.Count; j++)
                    {
                        connectionsId.Add(j, web[i][currentRewardedNodeId].connections[j]);
                    }

                    currentRewardedConnectionId = connectionsId.OrderByDescending(x => x.Value).First().Key;
                    rewardAmount = (1f - web[i][currentRewardedNodeId].connections[currentRewardedConnectionId]) / 8f;
                    web[i][currentRewardedNodeId].connections[currentRewardedConnectionId] += rewardAmount;

                    for (int j = 0; j < web[i][currentRewardedNodeId].connections.Count; j++)
                    {
                        if (currentRewardedConnectionId != j)
                        {
                            web[i][currentRewardedNodeId].connections[j] -= rewardAmount / (float)(web[i][currentRewardedNodeId].connections.Count - 1f);
                        }
                    }

                    currentRewardedNodeId = currentRewardedConnectionId;
                }
            }
        }

        public class Node
        {
            public float value = 0;
            public List<float> connections = new List<float>();

            public Node()
            {

            }

            public Node(int _numberOfConnections)
            {
                float[] preWeights = new float[_numberOfConnections];
                float sum;

                for (int i = 0; i < _numberOfConnections; i++)
                {
                    preWeights[i] = (float)rnd.Next(0, 1000000);
                }
                sum = preWeights.Sum();
                for (int i = 0; i < _numberOfConnections; i++)
                {
                    connections.Add(preWeights[i] / sum);
                }
            }
        }
    }
}