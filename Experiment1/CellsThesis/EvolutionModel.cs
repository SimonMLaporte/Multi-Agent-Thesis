using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellsThesis
{
    // This class is outdated!
    class EvolutionModel
    {
        public List<Genome> genomes;
        List<Genome> parents;
        List<Genome> offspring;
        List<Genome> mutatedGenomes;
        public Genome bestIndividual;
        public bool newGen;
        public int generation;

        public EvolutionModel(List<Genome> iGenomes)
        {
            genomes = new List<Genome>(iGenomes);
            newGen = true;

            // Assign fitness values to genomes
            for (int i = 0; i < genomes.Count; i++)
            {
                genomes[i].fitness = 0;
            }

        }

        public void Selection()
        {
            // Rank-based FPS sampling as scale of fitness is unknown
            // 1. Rank Genomes after fitness
            genomes.OrderBy(Genome => Genome.fitness);
            // Select Best individual
            bestIndividual = genomes[0];

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
                        parents.Add(genomes[i]);
                    }
                }

            }

        }

        public void Mutate(double mutationRate)
        {
            mutatedGenomes = new List<Genome>();
            Random r = new Random();
            for (int i = 0; i < offspring.Count; i++)
            {
                offspring[i].Mutate(mutationRate);
                mutatedGenomes.Add(offspring[i]);

            }

        }

        public void Crossover()
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

        public List<Genome> GetNewGenomes()
        {
            return mutatedGenomes;
        }

        public void ReplaceOldGenomes()
        {
            genomes.Clear();
            genomes = mutatedGenomes;
        }

        public bool StopCondition()
        {
            bool stop = false;
            return stop;
        }
    }
}
