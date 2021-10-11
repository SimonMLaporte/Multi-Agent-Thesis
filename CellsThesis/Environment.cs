using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Rhino.Geometry;
using System.Threading.Tasks;

namespace CellsThesis
{
    public class Environment
    {
        public int[,] cellLattice;
        double[,,] chemLattice;
        public int[,,] chemSpawners;
        double decay;
        public int numChem;
        int dimension;

        public Environment(int NumberChem, Bitmap Image, double Decay)
        {
            dimension = 200;
            numChem = NumberChem;
            decay = Decay;
            chemLattice = new double[200, 200, numChem];
            chemSpawners = new int[200, 200, numChem];
            cellLattice = new int[200, 200];

            double Threshold = 255 / (numChem+1);

            // assign bitmap colors to create chemaical spawners
            for (int i = 0; i < numChem; i++)
            {
                for (int j = 0; j < 200; j++)
                    for (int k = 0; k < 200; k++)
                    {
                        Color color = Image.GetPixel(j, k);
                        if ((i+1) * Threshold < color.R & color.R <= (i+2)*Threshold)
                        {
                            chemSpawners[j, k, i] = 25;
                        }
                    }
            }
        }

        public double GetValue(int ChemicalID, Point2d Position)
        {
            // TODO: Optimize by making the chemLattice larger
            double value;
            try
            {
                value = chemLattice[(int)Position.X, (int)Position.Y, ChemicalID];
            }
            catch
            {
                value = -10.0;
            }

            return value;
        }

        public void ClearLattice()
        {
            cellLattice = new int[200, 200];
            chemLattice = new double[200, 200, numChem];
        }

        public int GetId(int[] Position)
        {
            int ID = cellLattice[Position[0], Position[1]];
            return ID;
        }

        public void SetId(int[] Position, int ID)
        {
            cellLattice[Position[0], Position[1]] = ID;
        }

        public void Deposit(int[] Position, int DepT, int ChemID)
        {
            chemLattice[Position[0], Position[1], ChemID] += DepT;
        }

        public void Diffuse()
        {
            
            // simple median filter imp. Add Huangs algorithm instead
            List<double> neighbourhood = new List<double>();
            double[,,] newLat = new double[200, 200, numChem];
            int wx2 = 1;
            int wy2 = 1;
            for (int i = 0; i < numChem; i++)
            {
                for (int row = 1; row < 199; row++)
                {
                    for (int col = 1; col < 199; col++)
                    {
                        for (int r = row - wy2; r <= row + wy2; r++)
                        {
                            for (int c = col - wx2; c <= col + wx2; c++)
                            { neighbourhood.Add(chemLattice[r, c, i]); }
                        }
                        newLat[row, col, i] = neighbourhood.Sum() / 9;
                        neighbourhood.Clear();
                    }
                }
            }
            chemLattice = newLat;

            // add decay
            // add values from chemical spawners to environment
            for (int i = 0; i < numChem; i++)
                for (int j = 0; j < dimension; j++)
                    for (int k = 0; k < dimension; k++)
                    {
                        chemLattice[j, k, i] += chemSpawners[j, k, i];
                        chemLattice[j, k, i] *= decay;
                        chemLattice[j, k, i] = Math.Min(chemLattice[j, k, i], 255);
                    }

        }
    }
}
