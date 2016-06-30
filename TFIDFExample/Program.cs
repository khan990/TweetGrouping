//#define Cluster
//#define NeuralNetwork

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Accord.MachineLearning;
using Accord.Controls;
using Accord.Math;
using Accord.IO;
using Accord.Statistics;
using System.Windows.Forms;
using Accord.Neuro;
using AForge.Neuro.Learning;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;


namespace TFIDFExample
{
    class Program
    {
        static void Main(string[] args)
        {
            
            
#if Cluster
            // output file
            List<string> outputLines = new List<string>();

            DateTime timeStart = new DateTime();
            // Some example documents.
            string[] documents = new GetTweets().GetTweetsFromExcelFile("Train_NN.xlsx");

            // Apply TF*IDF to the documents and get the resulting vectors.
            double[][] inputs = TFIDF.Transform(documents, 0);
            Console.WriteLine("time to transformation " + (DateTime.Now - timeStart));
            outputLines.Add("time to transformation " + (DateTime.Now - timeStart));
            Console.WriteLine("TFIDF transformation done...");

            inputs = TFIDF.Normalize(inputs);
            Console.WriteLine("time to Normalization " + (DateTime.Now - timeStart));
            outputLines.Add("time to Normalization " + (DateTime.Now - timeStart));
            Console.WriteLine("TFIDF Normalization done...");
            //inputs = Accord.Math.Norm.Norm2(inputs);

            string[] topics = TFIDF.Topics(documents, 5);
            Console.WriteLine("time to topics " + (DateTime.Now - timeStart));
            outputLines.Add("time to topics " + (DateTime.Now - timeStart));
            Console.WriteLine("Topics gathered...");

            //Random random = new Random();
            //double[][] rand = new double[inputs.Length][];

            //for (int i = 0; i < inputs.Length; i++)
            //{

            //    rand[i] = new double[inputs[i].Length];
            //    for (int j = 0; j < inputs[i].Length; j++)
            //    {

            //        rand[i][j] = random.NextDouble();
            //    }
            //}
            //Console.WriteLine("time to generate random numbers " + (DateTime.Now - timeStart));
            //outputLines.Add("time to topics " + (DateTime.Now - timeStart));
            //Console.WriteLine("Randoms generated...");

            KMeans cluster = new KMeans(topics.Length, Distance.Cosine);

            //cluster.MaxIterations = 1;
            //cluster.Randomize(rand);
            int[] index = cluster.Compute(inputs);
            Console.WriteLine("time to cluster " + (DateTime.Now - timeStart));
            outputLines.Add("time to cluster " + (DateTime.Now - timeStart));
            Console.WriteLine("Clustering done...");
            //Accord.Statistics.Analysis.PrincipalComponentAnalysis pca = new Accord.Statistics.Analysis.PrincipalComponentAnalysis(inputs, Accord.Statistics.Analysis.AnalysisMethod.Center);
            //pca.Compute();
            //double[][] newinput = pca.Transform(inputs, 2);

            //ScatterplotBox.Show("KMeans Clustering of Tweets", newinput, index).Hold();

            

            for(double i=0; i<=topics.Length; i++)
            {
                outputLines.Add(Convert.ToString(i + 1));
                List<string> topicDecider = new List<string>();
                string[] topicString;

                int j = 0;
                foreach (int x in index)
                {
                    if (x == i + 1)
                    {
                        topicDecider.Add(documents[j]);
                    }
                    j++;
                }

                topicString = TFIDF.Topics(topicDecider.ToArray(), topicDecider.Count/2);

                if(topicString.Length == 0)
                {
                    outputLines.Add("--------------------------------------------------------");
                    outputLines.Add("TOPIC: other");
                    outputLines.Add("--------------------------------------------------------");
                }
                else
                {
                    outputLines.Add("--------------------------------------------------------");
                    outputLines.Add("TOPIC: " + topicString[0]);
                    outputLines.Add("--------------------------------------------------------");
                }

                j = 0;
                foreach (int x in index)
                {
                    if(x == i+1)
                    {
                        outputLines.Add("Tweet ID " + j + ":\t" + documents[j]);
                    }
                    j++;
                }
                outputLines.Add("");
                outputLines.Add("");
                outputLines.Add("");
                outputLines.Add("");

            }

            System.IO.File.WriteAllLines(@"Train_NN_2.txt", outputLines.ToArray());
            Console.WriteLine("Output is written...");
#else
            // output file
            List<string> outputLines = new List<string>();

            DateTime timeStart = new DateTime();
            // Some example documents.
            string[] documents_Train = new GetTweets().GetTweetsFromExcelFile("Train_NN.xlsx");
            double[][] Train_Labels = new GetTweets().GetLabelsFromExcelFile("Train_Labels.xlsx");

            // Apply TF*IDF to the documents and get the resulting vectors.
            double[][] inputs = TFIDF.Transform(documents_Train, 0);
            Console.WriteLine("time to transformation " + (DateTime.Now - timeStart));
            outputLines.Add("time to transformation " + (DateTime.Now - timeStart));
            Console.WriteLine("TFIDF transformation done...");

            inputs = TFIDF.Normalize(inputs);
            Console.WriteLine("time to Normalization " + (DateTime.Now - timeStart));
            outputLines.Add("time to Normalization " + (DateTime.Now - timeStart));
            Console.WriteLine("TFIDF Normalization done...");


            //double[][] inputs;
            double[][] train_input = new double[140][];
            double[][] outputs;
            double[][] testInputs = new double[1000-140][];
            double[][] testOutputs = new double[1000-140][];

            for(int i=0; i<140; i++)
            {
                train_input[i] = new double[inputs[i].Length];
                for(int j=0; j<inputs[i].Length; j++)
                {
                    train_input[i][j] = inputs[i][j];
                }
            }

            for(int i=0; i < 1000 - 140; i++)
            {
                testInputs[i] = new double[inputs[i].Length];
                for(int j=0; j < inputs[i].Length; j++)
                {
                    testInputs[i][j] = inputs[i][j];
                }
            }


            // The first 500 data rows will be for training. The rest will be for testing.
            //testInputs = inputs.Skip(500).ToArray();
            //testOutputs = outputs.Skip(500).ToArray();
            //inputs = inputs.Take(500).ToArray();
            //outputs = outputs.Take(500).ToArray();

            // Setup the deep belief network and initialize with random weights.
            DeepBeliefNetwork network = new DeepBeliefNetwork(train_input.First().Length, 7);
            new GaussianWeights(network, 0.1).Randomize();
            network.UpdateVisibleWeights();

            // Setup the learning algorithm.
            DeepBeliefNetworkLearning teacher = new DeepBeliefNetworkLearning(network)
            {
                Algorithm = (h, v, i) => new ContrastiveDivergenceLearning(h, v)
                {
                    LearningRate = 0.1,
                    Momentum = 0.5,
                    Decay = 0.001,
                }
            };

            // Setup batches of input for learning.
            int batchCount = Math.Max(1, train_input.Length / 100);
            // Create mini-batches to speed learning.
            int[] groups = Accord.Statistics.Tools.RandomGroups(train_input.Length, batchCount);
            double[][][] batches = train_input.Subgroups(groups);
            // Learning data for the specified layer.
            double[][][] layerData;

            // Unsupervised learning on each hidden layer, except for the output layer.
            for (int layerIndex = 0; layerIndex < network.Machines.Count - 1; layerIndex++)
            {
                teacher.LayerIndex = layerIndex;
                layerData = teacher.GetLayerInput(batches);
                for (int i = 0; i < 200; i++)
                {
                    double error = teacher.RunEpoch(layerData) / train_input.Length;
                    if (i % 10 == 0)
                    {
                        Console.WriteLine(i + ", Error = " + error);
                    }
                }
            }

            // Supervised learning on entire network, to provide output classification.
            var teacher2 = new BackPropagationLearning(network)
            {
                LearningRate = 0.1,
                Momentum = 0.5
            };
            
            //Transpose
            double[][] Train_Labels_T = new double[140][];
            for(int i=0; i<140; i++)
            {
                Train_Labels_T[i] = new double[7];
                for(int j=0; j<7; j++)
                {
                    Train_Labels_T[i][j] = Train_Labels[j][i];
                }
            }

            // Run supervised learning.
            for (int i = 0; i < 500; i++)
            {
                double error = teacher2.RunEpoch(train_input, Train_Labels_T) / train_input.Length;
                if (i % 10 == 0)
                {
                    Console.WriteLine(i + ", Error = " + error);
                }
            }
            outputLines.Add("time to Training " + (DateTime.Now - timeStart));
            // Test the resulting accuracy.
            double[][] outputValues = new double[testInputs.Length][];
            for (int i = 0; i < testInputs.Length; i++)
            {
                outputValues[i] = network.Compute(testInputs[i]);
               
            }
            outputLines.Add("time to Testing/clustering " + (DateTime.Now - timeStart));
            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");

            List<string> class1 = new List<string>();
            List<string> class2 = new List<string>();
            List<string> class3 = new List<string>();
            List<string> class4 = new List<string>();
            List<string> class5 = new List<string>();
            List<string> class6 = new List<string>();
            List<string> class7 = new List<string>();

            //creating output file
            for (int i=0; i< documents_Train.Length; i++)
            {
                if(i < 10 && i > -1)
                {
                    if (i == 0)
                    {
                        class1.Add("-------------------------------");
                        class1.Add("TOPIC: WEATHER");
                        class1.Add("-------------------------------");
                    }
                    class1.Add("Training_Tweet:\t" + documents_Train[i]);
                }
                if (i < 20 && i > 9)
                {
                    if (i == 10)
                    {
                        class2.Add("-------------------------------");
                        class2.Add("TOPIC: MUSIC");
                        class2.Add("-------------------------------");
                    }
                    class2.Add("Training_Tweet:\t" + documents_Train[i]);
                }
                if (i < 30 && i > 19)
                {
                    if (i == 20)
                    {
                        class3.Add("-------------------------------");
                        class3.Add("TOPIC: ITALY");
                        class3.Add("-------------------------------");
                    }
                    class3.Add("Training_Tweet:\t" + documents_Train[i]);
                }
                if (i < 40 && i > 29)
                {
                    if (i == 30)
                    {
                        class4.Add("-------------------------------");
                        class4.Add("TOPIC: FOOD");
                        class4.Add("-------------------------------");
                    }
                    class4.Add("Training_Tweet:\t" + documents_Train[i]);
                }
                if (i < 50 && i > 39)
                {
                    if (i == 40)
                    {
                        class5.Add("-------------------------------");
                        class5.Add("TOPIC: FASHION");
                        class5.Add("-------------------------------");
                    }
                    class5.Add("Training_Tweet:\t" + documents_Train[i]);
                }
                if (i < 60 && i > 49)
                {
                    if (i == 50)
                    {
                        class6.Add("-------------------------------");
                        class6.Add("TOPIC: FOOTBALL");
                        class6.Add("-------------------------------");
                    }
                    class6.Add("Training_Tweet:\t" + documents_Train[i]);
                }
                if (i < 140 && i > 59)
                {
                    if (i == 60)
                    {
                        class7.Add("-------------------------------");
                        class7.Add("TOPIC: OTHER");
                        class7.Add("-------------------------------");
                    }
                    class7.Add("Training_Tweet:\t" + documents_Train[i]);
                }
                if(i >= 140)
                {
                    int what;
                    what = outputValues[i-140].IndexOf(outputValues[i - 140].Max() );
                    switch (what)
                    {
                        case 0:
                            class1.Add("Test_Tweet:\t" + documents_Train[i]);
                            break;

                        case 1:
                            class2.Add("Test_Tweet:\t" + documents_Train[i]);
                            break;

                        case 2:
                            class3.Add("Test_Tweet:\t" + documents_Train[i]);
                            break;

                        case 3:
                            class4.Add("Test_Tweet:\t" + documents_Train[i]);
                            break;

                        case 4:
                            class5.Add("Test_Tweet:\t" + documents_Train[i]);
                            break;

                        case 5:
                            class6.Add("Test_Tweet:\t" + documents_Train[i]);
                            break;

                        case 6:
                            class7.Add("Test_Tweet:\t" + documents_Train[i]);
                            break;
                    }
                }
            }

            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");
            outputLines.AddRange(class1);
            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");
            outputLines.AddRange(class2);
            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");
            outputLines.AddRange(class3);
            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");
            outputLines.AddRange(class4);
            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");
            outputLines.AddRange(class5);
            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");
            outputLines.AddRange(class6);
            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");
            outputLines.AddRange(class7);
            outputLines.Add("");
            outputLines.Add("");
            outputLines.Add("");


            System.IO.File.WriteAllLines(@"Train_NN_With_Test_2.txt", outputLines.ToArray());

            Console.Write("Press any key to quit ..");
#endif

            Console.ReadKey();
        }


        
    }
}
