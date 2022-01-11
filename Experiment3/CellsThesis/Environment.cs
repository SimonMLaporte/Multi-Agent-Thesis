using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Grasshopper;
using Rhino.Geometry;
using System.Threading.Tasks;

namespace CellsThesis
{
    public class Environment
    {
        public int[,,] cellLattice;
        public static int[,,] binLattice;
        public int[,] obstacleLattice;
        double[,,] chemLattice;
        public static int[,,] neighbourLattice9;
        public static int[,,] neighbourLattice5;
        public double[,,] chemSpawners;
        double decay;
        public int numChem;
        public int materialNumber;

        public DataTree<int> interfaceMap;


        public Environment(int NumberChem, Bitmap Image, Bitmap Image2, Bitmap Image3, Bitmap Collision, double Decay, int MaterialNumber)
        {
            // Initialize bitmaps
            List<Bitmap> images = new List<Bitmap>() { Image, Image2, Image3, Collision };
            for (int i = 0; i < images.Count; i++)
                images[i].RotateFlip(RotateFlipType.Rotate180FlipNone);

            numChem = NumberChem;
            decay = Decay;

            // Initialize data maps
            chemLattice = new double[202, 202, numChem];
            chemSpawners = new double[200, 200, numChem];
            neighbourLattice9 = new int[200, 200, numChem];
            neighbourLattice5 = new int[200, 200, numChem];
            binLattice = new int[10, 10, 3];
            cellLattice = new int[200, 200, numChem];
            interfaceMap = new DataTree<int>();
            // initialize interface Maps
            for (int i = 0; i < 3; i++)
            {
                Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(i);
                List<int> empty = new List<int>();
                for (int j = 0; j < 200 * 200; j++)
                {
                    empty.Add(0);
                }
                interfaceMap.AddRange(empty, path);
            }


            obstacleLattice = new int[200, 200];
            materialNumber = MaterialNumber;

            // set global parameters (These could be done as static objects instead);
            Globals.chemNumber = NumberChem;
            Globals.materialNumber = MaterialNumber;


            double Threshold = 255.0 / (numChem + 1);
            double attractStrength = 20.0; // maximal attractor strength
            double repellentStrength = -20.0;


            // image 1 = disc 1, image 2 = disc 2 , image 3 = obstacle
            // if only red values = repulsion,
            // if white value > 50 attraction
            for (int i = 0; i < numChem + 1; i++)
            {
                for (int j = 0; j < images[i].Width; j++)
                    for (int k = 0; k < images[i].Height; k++)
                    {
                        if (i == numChem)
                        {
                            Color oColor = images[numChem].GetPixel(j, k);
                            if (oColor.G > 50)
                                obstacleLattice[j, k] = 1;
                        }
                        else
                        {
                            Color color = images[i].GetPixel(j, k);
                            if (color.R > 1 & color.G == 0)
                            {
                                //chemSpawners[j, k, 0] = attractStrength*(color.R-50)/(255-50);
                                //chemSpawners[j, k, 1] = attractStrength * (color.R - 50) / (255 - 50);
                                chemSpawners[j, k, i] = repellentStrength * (color.R - 1) / (255 - 1);
                            }
                            else
                            {
                                //chemSpawners[j, k, 0] = repellentStrength*(color.R-50)/(255-50);
                                //chemSpawners[j, k, 1] = repellentStrength * (color.R - 50) / (255 - 50);
                                chemSpawners[j, k, i] = attractStrength * (color.B) / (255);
                            }
                        }



                    }
            }


        }

        public double GetValue(int ChemicalID, Point2d Position)
        {
            // TODO: Optimize by making the chemLattice larger
            double value;

            if (Position.X > 0 & Position.X < 200 & Position.Y > 0 & Position.Y < 200)
                value = chemLattice[(int)Position.X + 1, (int)Position.Y + 1, ChemicalID];
            else
                value = 0.0;
            return value;
        }

        public void ClearLattice()
        {
            cellLattice = new int[200, 200, numChem];
            chemLattice = new double[202, 202, numChem];
        }

        public int GetId(int[] Position, int material)
        {

            if (obstacleLattice[Position[0], Position[1]] == 1 & material == 0)
                return 1;
            else
            {
                int ID = cellLattice[Position[0], Position[1], material];
                return ID;
            }




        }

        public void SetId(int[] Position, int ID, int material)
        {

            cellLattice[Position[0], Position[1], material] = ID;

        }

        public void Deposit(int[] Position, double DepT, int ChemID)
        {
            chemLattice[Position[0] + 1, Position[1] + 1, ChemID] += DepT;
        }
        public void Cull(int[] Position, int ChemID)
        {


            chemLattice[Position[0] + 1, Position[1] + 1, ChemID] = 0.0;



        }

        public void Diffuse()
        {

            //simple median filter imp. Add Huangs algorithm instead
            List<double> neighbourhood = new List<double>();
            double[,,] newLat = new double[202, 202, numChem];
            int wx2 = 1;
            int wy2 = 1;
            //initialize edge by duplicting nearest value for edge persevation
            for (int i = 0; i < numChem; i++)
            {
                for (int j = 1; j < 201; j++)
                {
                    chemLattice[j, 0, i] = chemLattice[j, 1, i];//Bottom edge
                    chemLattice[0, j, i] = chemLattice[1, j, i]; //Left Edge
                    chemLattice[201, j, i] = chemLattice[200, j, i]; //Right Edge
                    chemLattice[j, 201, i] = chemLattice[j, 200, i]; //Right Edge
                }

            }
            //Initialize Corners
            for (int i = 0; i < numChem; i++)
            {
                chemLattice[0, 0, i] = chemLattice[1, 1, i];
                chemLattice[201, 201, i] = chemLattice[200, 200, i];
                chemLattice[0, 201, i] = chemLattice[1, 200, i];
                chemLattice[201, 0, i] = chemLattice[200, 1, i];

            }


            for (int i = 0; i < numChem; i++)
            {
                for (int row = 1; row < 201; row++)
                {
                    for (int col = 1; col < 201; col++)
                    {
                        for (int r = row - wy2; r <= row + wy2; r++)
                        {
                            for (int c = col - wx2; c <= col + wx2; c++)
                            { neighbourhood.Add(chemLattice[r, c, i]); }
                        }
                        newLat[row, col, i] = neighbourhood.Sum() / 9.0;
                        neighbourhood.Clear();
                    }
                }
            }
            chemLattice = newLat;







            // add values from chemical spawners to environment
            for (int i = 0; i < numChem; i++)
                for (int j = 1; j < 201; j++) //X
                    for (int k = 1; k < 201; k++) //Y
                    {

                        if (obstacleLattice[j - 1, k - 1] == 1)
                            chemLattice[j, k, 0] = 0;
                        chemLattice[j, k, i] += chemSpawners[j - 1, k - 1, i];
                        chemLattice[j, k, i] *= decay;
                        chemLattice[j, k, i] = Math.Min(chemLattice[j, k, i], 255.0);
                        chemLattice[j, k, i] = Math.Max(chemLattice[j, k, i], -400.0);


                    }

        }

        public void FindNeighbours(int offset, int material) // offset can only be 2 or 4
        {
            int[,,] newLat = new int[200, 200, 2];
            int wx2 = offset;
            int wy2 = offset;

            for (int row = 0; row < 199; row++)
            {
                for (int col = 0; col < 199; col++)
                {
                    if (cellLattice[row, col, material] > 0)
                    {
                        for (int r = row - wy2; r <= row + wy2; r++)
                        {
                            for (int c = col - wx2; c <= col + wy2; c++)
                            {
                                if (c >= 0 & c < 200 & r >= 0 & r < 200)
                                {

                                    if (cellLattice[r, c, material] > 0)
                                    {
                                        newLat[row, col, material]++;
                                    }


                                }

                            }
                        }
                    }

                }
            }

            if (offset == 4)
                neighbourLattice9 = newLat;
            else if (offset == 2)
                neighbourLattice5 = newLat;

        }

        public void CalculateInterfaceMap()
        {
            for (int p = 0; p < 3; p++)
            {
                // Decide what material to compare with for each interface map
                int comparison;
                if (p == 0)
                {
                    comparison = 1;
                }
                else if (p == 1)
                {
                    comparison = 2;
                }
                else { comparison = 0; }


                for (int i = 0; i < 199; i++)//X-values
                {
                    for (int j = 0; j < 199; j++) //Y-values
                    {
                        int count = 0;
                        if (cellLattice[i, j, p] > 0)
                            count++;
                        if (cellLattice[i, j, comparison] > 0)
                            count++;
                        if (count > 1)
                            interfaceMap.Branch(p)[i * 200 + j] += count - 1;


                    }
                }


            }


        }

        public void ClearBinLattice()
        {

            binLattice = new int[10, 10, 3];
        }
    }
}
   
