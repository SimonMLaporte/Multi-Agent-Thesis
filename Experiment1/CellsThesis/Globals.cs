using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellsThesis
{
    public static class Globals
    {
        // Utilities
        public static Random r = new Random();
        public static int t = 0;
        private static TimeSpan ts;
        private static String elapsedTime;
        private static Stopwatch stopwatch;

        //Model Control
        public static bool simFinisehd;
        public static int generation;

        // Evolutio Genomes
        public static List<Genome> genomes;
        static List<Genome> parents;
        static List<Genome> offspring;
        static List<Genome> mutatedGenomes;
        static List<Genome> eliteIndividuals;
        public static Genome bestIndividual;
        

       

        public static void InitClock()
        {
            stopwatch = new Stopwatch();
        }

        public static void StartClock()
        {
            stopwatch.Reset();
            stopwatch.Start();

        }

        public static String StopClock()
        {
            stopwatch.Stop();
            ts = stopwatch.Elapsed;
            elapsedTime = String.Format("{0:00}.{1:00}",
            ts.Seconds,
            ts.Milliseconds / 10);
            return elapsedTime;
        }

        public static void Selection(int elitism)
        {
         
            // Rank-based FPS sampling as scale of fitness is unknown
            // 1. Rank Genomes after fitness
            List<Genome> sortedGenomes = genomes.OrderBy(Genome => Genome.fitness).ToList();
            sortedGenomes.Reverse();

            eliteIndividuals = new List<Genome>();
            // Add elite individuals - These individuals replace random genomes after mutation
            for (int i = 0;i<elitism;i++)
            {
                eliteIndividuals.Add(sortedGenomes[i]);
            }

            // Select Best individual
            bestIndividual = sortedGenomes[0];

            // 2. Calculate fraction of selection
            // Total ranking
            double totalRank = 0.0;
            for (int i = 1; i < genomes.Count + 1; i++)
            {
                totalRank += i;
            }
            List<int> number = new List<int>();

            // 3. Populate selection list
            for (int i = 0; i < genomes.Count; i++)
            {
                double rank = genomes.Count - i;
                number.Add((int)Math.Round(genomes.Count * rank / totalRank));
            }


            parents = new List<Genome>();
            for (int i = 0; i < genomes.Count; i++)
            {
                if (parents.Count < genomes.Count)
                {
                    for (int j = 0; j < number[i]; j++)
                    {
                        parents.Add(sortedGenomes[i]);
                    }
                }

            }

        }

        public static void Mutate(double mutationRate)
        {
            mutatedGenomes = new List<Genome>();
            for (int i = 0; i < offspring.Count; i++)
            {
                offspring[i].Mutate(mutationRate);
                mutatedGenomes.Add(offspring[i]);

            }
            for (int i = 0;i<eliteIndividuals.Count;i++)
            {
                mutatedGenomes.RemoveAt(0);
                mutatedGenomes.Add(eliteIndividuals[i]);
            }
        }

        public static void Crossover()
        {
            Random rng = new Random();
            offspring = new List<Genome>();
            //Single point crossover implemented, change after genome is changed
            for (int i = 0; i < genomes.Count; i++)
            {
                int parentA = rng.Next(0, genomes.Count - 1);
                int parentB = rng.Next(0, genomes.Count - 1);

                Genome child = parents[parentA].Crossover(parents[parentB]);
                child.fitness = 0;
                offspring.Add(child);
            }
        }

        public static List<Genome> GetNewGenomes()
        {
            return mutatedGenomes;
        }

        public static void ReplaceOldGenomes()
        {
            genomes.Clear();
            genomes = mutatedGenomes;
        }

        public static bool StopCondition()
        {
            bool stop = false;
            return stop;
        }
    }
}
