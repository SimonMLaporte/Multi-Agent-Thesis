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

        public int[,,,] DNA;

        public double fitness;
        int numGenes;

        public Genome()
        {

            numGenes = 14;
            DNA = new int[numGenes, 3,Globals.faceCount,Globals.materialNumber];
            fitness = 0;
            

            // Initialize genome
            // for each face
            for (int j = 0;j<Globals.faceCount;j++)
            {
                for (int k = 0;k<Globals.materialNumber;k++)
                {
                    for (int i = 0; i < numGenes; i++)
                    {
                        // initialize constants in range 0 to 255
                        DNA[i, 0,j,k] = Globals.r.Next(0, 256);
                        // initialize what constant to compare chemValue 1 = 0, chemValue 2 = 1, chemvalue 3 = 2, t = 3;, size 1 = 4, FaceSize = 5, x = 6, y = 7, 8 = direction
                        DNA[i, 1,j,k] = Globals.r.Next(0, 8 + 1);
                        // Initialize what method to use Smaller than = 0, , larger than = 1, Always = 2, never = 3
                        DNA[i, 2,j,k] = Globals.r.Next(0, 3 + 1);
                    }
                }
                
            }
            
        }

        public Genome(int[,,,] iDNA)
        {
            fitness = 0;
            DNA = iDNA;
        }
        public Genome Crossover(Genome ParentB)
        {
            // Implement Blend Crossover and uniform crossover
            Genome child = new Genome();
            //1-point Crossover
            for (int j = 0; j<Globals.faceCount;j++)
                for (int k = 0; k<Globals.materialNumber;k++)
                {
                    int midpoint = (int)Globals.r.Next(0, numGenes);
                    for (int i = 0; i < numGenes; i++)
                    {
                        if (i > midpoint)
                        {
                            child.DNA[i, 0,j,k] = DNA[i, 0,j,k];
                            child.DNA[i, 1,j,k] = DNA[i, 1,j,k];
                            child.DNA[i, 2,j,k] = DNA[i, 2,j,k];
                        }
                        else
                        {
                            child.DNA[i, 0,j,k] = ParentB.DNA[i, 0,j,k];
                            child.DNA[i, 1,j,k] = ParentB.DNA[i, 1,j,k];
                            child.DNA[i, 2,j,k] = ParentB.DNA[i, 2,j,k];
                        }
                    }
                }
            
            return child;
        }

        public void Mutate(double mutationRate)
        {
            for (int j =0;j<Globals.faceCount;j++)
                for(int k = 0;k<Globals.materialNumber;k++)
                {
                    for (int i = 0; i < DNA.GetLength(0); i++)
                    {
                        if (Globals.r.NextDouble() < mutationRate)
                        {
                            //mutate constants
                            DNA[i, 0,j,k] = Globals.r.Next(0, 255 + 1);
                        }
                        if (Globals.r.NextDouble() < mutationRate)
                        {   // mutate variable
                            DNA[i, 1,j,k] = Globals.r.Next(0, 7 + 1);
                        }
                        if (Globals.r.NextDouble() < mutationRate)
                        {
                            // mutate method
                            DNA[i, 2,j,k] = Globals.r.Next(0, 3 + 1);
                        }
                    }

                }
            if (Globals.r.NextDouble() < mutationRate*10) // Swap two faces
            {
                int indexA = Globals.r.Next(0, Globals.faceCount + 1);
                int indexB = Globals.r.Next(0, Globals.faceCount + 1);

                int[,,] tempDNA = new int[numGenes,3,Globals.materialNumber];
                for (int i = 0; i < numGenes; i++)
                    for (int j = 0; j < 3; j++)
                        for (int k = 0; k < Globals.materialNumber; k++)
                            tempDNA[i, j, k] = DNA[i, j, indexA, k];

                for (int i = 0; i < numGenes; i++)
                    for (int j = 0; j < 3; j++)
                        for (int k = 0; k < Globals.materialNumber; k++)
                            DNA[i, j, indexA, k] = DNA[i, j, indexB, k];

                for(int i = 0; i < numGenes; i++)
                    for (int j = 0; j < 3; j++)
                        for (int k = 0; k < Globals.materialNumber; k++)
                            DNA[i, j, indexA, k] = DNA[i, j, indexB, k];

                for(int i = 0; i < numGenes; i++)
                    for (int j = 0; j < 3; j++)
                    for (int k = 0; k < Globals.materialNumber; k++)
                        DNA[i, j, indexB, k] = tempDNA[i, j, k];

            }

            if (Globals.r.NextDouble() < mutationRate * 10) // Duplication
            {
                int indexA = Globals.r.Next(0, Globals.faceCount + 1);
                int indexB = Globals.r.Next(0, Globals.faceCount + 1);

                for (int i = 0; i < numGenes; i++)
                    for (int j = 0; j < 3; j++)
                        for (int k = 0; k < Globals.materialNumber; k++)
                            DNA[i, j, indexA, k] = DNA[i, j, indexB, k];

            }
        }

    }
}
