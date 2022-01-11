using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using System.Threading.Tasks;

namespace CellsThesis
{
    public class Cell
    {
        public Point2d positionF;
        public int id;
        public Vector2d direction;
        public int[] positionD = new int[2];
        int[] posLattice = new int[2];
        public int curFace;
        
        public double size;


        //Constants
        private double SS = 1; // speed
        private double SO = 12; // sensory offset;
        private double deptT = 2.0; //Deposited chemical per step

        // Behavoirs logix
        bool[] behavoir;
        delegate bool logic(int var, int c);
        logic[] delegates;

        // simulation constants
        public double[,] chemValues;
        public int material;
        int oMaterial;
        int[,,,] DNA;
        int t;

        public Cell(Point2d Position, int ID, Genome Genome, int iMaterial)
        {
            material = iMaterial;

            if (material == 0)
                oMaterial = 1;
            else oMaterial = 0;

            // Create list of methods
            delegates = new logic[]
            {
                new logic(SmallerThan),
                new logic(LargerThan),
                new logic(Always),
                new logic(Never)

            };

            // Initialize Cell parameters

            DNA = Genome.DNA;
            behavoir = new bool[DNA.GetLength(0)];
            positionF = Position;
            id = ID;
            t = 0;
            positionD[0] = (int)Position.X;
            positionD[1] = (int)Position.Y;
            double range = 2 * 3.14;
            direction = new Vector2d(1, 0);
            direction.Rotate(Globals.r.NextDouble() * range);
            chemValues = new double[3, Globals.chemNumber];
            size = 0.0;
        }

        public void BinLattice()
        {
            posLattice[0] = positionD[0] / 20;
            posLattice[1] = positionD[1] / 20;
            Environment.binLattice[posLattice[0], posLattice[1],material]++;
        }

        public void Decide()
        {
            int mat0Density = Environment.binLattice[posLattice[0], posLattice[1], 0];
            int mat1Density = Environment.binLattice[posLattice[0], posLattice[1], 1];
            int mat2Density = Environment.binLattice[posLattice[0], posLattice[1], 2];

            double anglePolarDegrees = Math.Atan2(direction.Y, direction.X) * 180 / 3.14;
            if (anglePolarDegrees < 0)
                anglePolarDegrees += 360;
            
            // For each gene decide if the value is true or false for the current timestep
            double[] vari = new double[] { chemValues[0, 0], chemValues[0, 1], chemValues[0, 2], mat0Density, mat1Density, mat2Density, positionF.X, positionF.Y, anglePolarDegrees };
            for (int i = 0; i < DNA.GetLength(0); i++)
            {
                bool des = delegates[DNA[i, 2, curFace, material]]((int)vari[DNA[i, 1, curFace, material]], DNA[i, 0, curFace, material]);
                behavoir[i] = des;
            }
            t++;
        }
        // TODO: Add more logic
        bool SmallerThan(int var, int c)
        {
            return var < c;
        }
        bool LargerThan(int var, int c)
        {
            return var > c;
        }
        bool Always(int var, int c)
        {
            return true;
        }
        bool Never(int var, int c)
        {
            return false;
        }

        public void DetermineRuleSet(List<Point2d> centerPoints)
        {
            List<double> distanceList = new List<double>();
            for (int i = 0; i < centerPoints.Count; i++)
            {
                distanceList.Add(positionF.DistanceTo(centerPoints[i]));
            }

            double minValue = distanceList.Min();
            curFace = distanceList.IndexOf(minValue);



        }
        public void Sense(Environment environment)
        {
            Point2d Fpos;
            Point2d FLpos;
            Point2d FRpos;
            //TODO: Optimize // only call when needed
            Fpos = positionF + direction * SO;
            direction.Rotate(Math.PI / 8);
            FLpos = positionF + direction * SO;
            direction.Rotate(-Math.PI / 4);
            FRpos = positionF + direction * SO;
            direction.Rotate(Math.PI / 8);


            for (int i = 0; i < Globals.chemNumber; i++)
            {
                chemValues[0, i] = environment.GetValue(i, Fpos);
                chemValues[1, i] = environment.GetValue(i, FLpos);
                chemValues[2, i] = environment.GetValue(i, FRpos);
            }

            if (chemValues[0, material] > 5)
            {
                size += 2;
                if (size > 30)
                    size = 0;
            }



            Steer();

        }

        public void Steer()
        {
            

            if (behavoir[11])
            { //steer towards material 0
                if (chemValues[1, 0] > chemValues[0, 0] & chemValues[1, 0] > chemValues[2, 0])
                    direction.Rotate(Math.PI / 8);

                if (chemValues[2, 0] > chemValues[0, 0] & chemValues[2, 0] > chemValues[1, 0])
                    direction.Rotate(-Math.PI / 8);
            }

            if (behavoir[12])
            { // Always steer away material 0
                if (chemValues[1, 0] > chemValues[0, 0] & chemValues[1, 0] > chemValues[2, 0])
                    direction.Rotate(-Math.PI / 8);

                if (chemValues[2, 0] > chemValues[0, 0] & chemValues[2, 0] > chemValues[1, 0])
                    direction.Rotate(Math.PI / 8);
            }

            if (behavoir[3])
            { //steer towards material 1
                if (chemValues[1, 1] > chemValues[0, 1] & chemValues[1, 1] > chemValues[2, 1])
                    direction.Rotate(Math.PI / 8);

                if (chemValues[2, 1] > chemValues[0, 1] & chemValues[2, 1] > chemValues[1, 1])
                    direction.Rotate(-Math.PI / 8);
            }
            if (behavoir[4])
            { // Always steer away material 1
                if (chemValues[1, 1] > chemValues[0, 1] & chemValues[1, 1] > chemValues[2, 1])
                    direction.Rotate(-Math.PI / 8);

                if (chemValues[2, 1] > chemValues[0, 1] & chemValues[2, 1] > chemValues[1, 1])
                    direction.Rotate(Math.PI / 8);
            }




            if (behavoir[1]) // Steer towards material 2
            {

                if (chemValues[1, 2] > chemValues[0, 2] & chemValues[1, 2] > chemValues[2, 2])
                    direction.Rotate(Math.PI / 8);

                if (chemValues[2, 2] > chemValues[0, 2] & chemValues[2, 2] > chemValues[1, 2])
                    direction.Rotate(-Math.PI / 8);

            }
            else

            if (behavoir[2]) // Steer away material 2
            {
                if (chemValues[1, 2] > chemValues[0, 2] & chemValues[1, 2] > chemValues[2, 2])
                    direction.Rotate(-Math.PI / 8);
                if (chemValues[2, 2] > chemValues[0, 2] & chemValues[2, 2] > chemValues[1, 2])
                    direction.Rotate(Math.PI / 8);


            }
        }

        public void Move(Environment environment)
        {
            Point2d newPos = positionF + direction;
            int[] newPosD = new int[2];
            newPosD[0] = (int)Math.Round(newPos.X);
            newPosD[1] = (int)Math.Round(newPos.Y);
            /////
            //Enforce bounds
            if (newPosD[0] < 200 & newPosD[1] < 200 & newPosD[0] > 0 & newPosD[1] > 0)
            {
                // Check if position is empty or environment

                if (environment.GetId(newPosD, material) == 0)
                {
                    environment.SetId(positionD, 0, material); // Clear old cell
                    positionF = newPos; // update position
                    environment.SetId(newPosD, id, material);
                    positionD = newPosD;
                    Deposit(environment);
                }
                else if (environment.GetId(newPosD, material) == id) //if movement does not reach new lattice
                {
                    positionF = newPos;
                    Deposit(environment);
                }
                else // New Random direction if cell is occupied
                {

                    Random r = new Random();
                    double range = 2.0 * Math.PI;

                    direction.Rotate(r.NextDouble() * range);
                }



            }
            else //turn around if movement is out of bounds
                direction *= -1.0;

        }

        public void Deposit(Environment environment)
        {


            if (behavoir[0])
            {
                environment.Deposit(positionD, deptT, material);
                
            }
            if (behavoir[5])
                environment.Deposit(positionD, deptT/2, 1);
            if (behavoir[6])
                environment.Deposit(positionD, deptT*2, 2);
            else
            {
                if (behavoir[7])
                    environment.Cull(positionD, 1);
            }
            if (behavoir[8])
                environment.Cull(positionD, 2);


        }

        public void Represent()
        {

            if (behavoir[13])
            {
                if (material == 0)
                {
                    oMaterial = 0;
                    material = 1;
                }
                else
                {
                    oMaterial = 1;
                    material = 0;
                }

            }
        }


        public int[] MetaDeposit(int[] iValues)
        {
            if (behavoir[6]) //set correct behavior reference
            {
                int[] adjustValues = iValues;
                adjustValues[curFace] += 1;
                return adjustValues;

            }
            else if (behavoir[6])
            {
                int[] adjustValues = iValues;
                adjustValues[curFace] -= 1;
                return adjustValues;
            }

            else { return iValues; }

        }
        public void SetSO()
        {
            if (behavoir[9])
            {
                SO = 5;
            }
            else if (behavoir[10])
            { SO = 24; }
        }

        public void Eat(Environment environment)
        {
            if (material == 0)
            {
                int offset = CellModel.cullOffset;
                for (int row = positionD[0] - offset; row < positionD[0] + offset; row++)
                    for (int col = positionD[1] - offset; col < positionD[1] + offset; col++)
                    {
                        if (row >= 0 & row < 200 & col >= 0 & col < 200)
                            environment.Deposit(new int[] { row, col }, -0.1, 1);
                    }
            }


        }


        // ------------------------------------ Below Methods are currently unused-----------------------------------------





    }
}
