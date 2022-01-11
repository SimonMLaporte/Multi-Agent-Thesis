using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellsThesis
{
    public class Genome
    {
        /* Output index:
        0. Deposit chem1
        1. Deposit chem2
        ... Deposit chem_n
        n   Steer chem1
        ... Steer chem_n
        2*n steer away 1
        ... Steer_away_chem_n
        3*n represent material 1
        ... represent material_k
        3*n+k stop
        */

        public int[,] DNA;
        public double fitness;
        int numGenes;
        public int materialNumber;
        public int chemNumber;

        public Genome(int ChemNumber, int MaterialNumber)
        {
            materialNumber = MaterialNumber;
            chemNumber = ChemNumber;
            numGenes = chemNumber * 3 + materialNumber +1; //+1 for stop
            DNA = new int[numGenes, 3];
            fitness = 0;
            

            // Initialize genome
            for (int i = 0;i<numGenes;i++)
            {
                // initialize constants in range 0 to 255
                DNA[i, 0] = Globals.r.Next(0, 256);
                // initialize what constant to compare x = 0, y = 1, t = 2;, material 1 = 3 material n = 2+n
                DNA[i, 1] = Globals.r.Next(0, 2+1);
                // Initialize what method to use Smaller than = 0, smaller than AND = 1, larger than = 2, larger than AND = 3, never = 4, always = 5
                DNA[i, 2] = Globals.r.Next(0, 5+1);
            }
        }
        public Genome Crossover(Genome ParentB)
        {
            // Implement Blend Crossover and uniform crossover
            Genome child = new Genome(chemNumber,materialNumber);
            //Midpoint Crossover
            int midpoint = (int)Globals.r.Next(0, numGenes);
            for (int i =0; i<numGenes;i++)
            {
                if (i>midpoint)
                {
                    child.DNA[i, 0] = DNA[i, 0];
                    child.DNA[i, 1] = DNA[i, 1];
                    child.DNA[i, 2] = DNA[i, 2];
                }
                else
                {
                    child.DNA[i, 0] = ParentB.DNA[i, 0];
                    child.DNA[i, 1] = ParentB.DNA[i, 1];
                    child.DNA[i, 2] = ParentB.DNA[i, 2];
                }
            }
            return child;
        }

        public void Mutate(double mutationRate)
        {
            for (int i = 0;i < DNA.GetLength(0);i++)
            {
                if (Globals.r.NextDouble()<mutationRate)
                {
                    //mutate constants
                    DNA[i,0] = Globals.r.Next(0, 255+1);
                }
                if (Globals.r.NextDouble() < mutationRate)
                {   // mutate variable
                    DNA[i, 1] = Globals.r.Next(0, 2+1);
                }
                if (Globals.r.NextDouble() < mutationRate)
                {
                    // mutate method
                    DNA[i, 2] = Globals.r.Next(0, 5+1);
                }
            }
        }

    }
}
