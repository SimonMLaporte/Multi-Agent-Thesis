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

        //Constants
        private double SS = 1; // speed
        private double SO = 12; // sensory offset;
        private int deptT = 1; //Deposited chemical per step
       
        // Behavoirs logix
        bool[] behavoir;
        delegate bool logic(int var, int c);
        logic[] delegates;

        // simulation constants
        double[,] chemValues;
        int numChem;
        public int material;
        int materialCount;
        int[,] DNA;
        int t;
        public bool run;

        public Cell(Point2d Position, int ID, Genome Genome)
        {
            material = 0;
            
            // Create list of methods
            delegates = new logic[]
            {
                new logic(SmallerThan),
                new logic(SmallerThan),
                new logic(LargerThan),
                new logic(LargerThan),
                new logic(Always),
                new logic(Never)

            };

            // Initialize Cell parameters
            run = true;
            DNA = Genome.DNA;
            materialCount = Genome.materialNumber;
            behavoir = new bool[DNA.GetLength(0)];
            positionF = Position;
            numChem = Genome.chemNumber;
            id = ID;
            t = 0;
            positionD[0] = (int)Position.X;
            positionD[1] = (int)Position.Y;
            Random r = new Random();
            double range = 2 * 3.14;
            direction = new Vector2d(1, 0);
            direction.Rotate(r.NextDouble() * range);
            chemValues = new double[3, numChem];

        }

        public void Decide()
        {
            // For each gene decide if the value is true or false for the current timestep
            int[] var = new int[] { positionD[0], positionD[1], t };
            for (int i=0;i<DNA.GetLength(0);i++)
            {
                bool des = delegates[DNA[i,2]](var[DNA[i, 1]], DNA[i, 0]);
                behavoir[i] = des;
            }
            t++;
        }
        // TODO: Add more logic
        bool SmallerThan (int var, int c)
        {
            return var < c;
        }
        bool LargerThan (int var, int c)
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


        public void Sense(Environment environment)
        {
            Point2d Fpos;
            Point2d FLpos;
            Point2d FRpos;
            //TODO: Optimize
            Fpos = positionF + direction * SO;
            direction.Rotate(Math.PI / 8);
            FLpos = positionF + direction * SO;
            direction.Rotate(-Math.PI / 4);
            FRpos = positionF + direction * SO;
            direction.Rotate(Math.PI / 8);



            for (int i = 0; i < numChem; i++)
            {
                chemValues[0, i] = environment.GetValue(i, Fpos);
                chemValues[1, i] = environment.GetValue(i, FLpos);
                chemValues[2, i] = environment.GetValue(i, FRpos);
            }
            Steer();

        }

        public void Steer()
        {
            // Steer
            for (int i = 0; i<numChem;i++)
            {
                if (behavoir[i+numChem])
                {
                    // Left
                    if (chemValues[1, i] > chemValues[0, i] & chemValues[1, i] > chemValues[2, i])
                        direction.Rotate(Math.PI / 8);
                   
                    if (chemValues[2, i] > chemValues[0, i] & chemValues[2, i] > chemValues[1, i])
                        direction.Rotate(-Math.PI / 8);

                }
            }

            // Steer away
            for (int i = 0; i < numChem; i++)
            {
                if (behavoir[i + 2*numChem])
                {
                    if (chemValues[1, i] > chemValues[0, i] & chemValues[1, i] > chemValues[2, i])
                        direction.Rotate(-Math.PI / 8);
                    if (chemValues[2, i] > chemValues[0, i] & chemValues[2, i] > chemValues[1, i])
                        direction.Rotate(Math.PI / 8);
                }
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
                if (environment.GetId(newPosD) == 0) 
                {
                    environment.SetId(positionD, 0); // Clear old cell
                    positionF = newPos; // update position
                    environment.SetId(newPosD, id);
                    positionD = newPosD;
                    Deposit(environment);
                }
                else if (environment.GetId(newPosD) == id) //if movement does not reach new lattice
                {
                    positionF = newPos;
                    Deposit(environment);
                }
                else // New Random direction if cell is occupied
                {
                    Random r = new Random();
                    double range = 2 * Math.PI;
                    ;
                    direction.Rotate(r.NextDouble() * range);
                }
            }
            else //turn around if movement is out of bounds
                direction *= -1;

        }

        public void Deposit(Environment environment)
        {
            // only deposit in first lattice so far //TODO
            for (int i =0;i<numChem;i++)
            {
                if (behavoir[i])
                {
                    environment.Deposit(positionD, deptT, i);
                }
            }
            
        }

        public void Stop()
        {
            if (behavoir[behavoir.Length-1])
                run = false;
        }
        
        public void Represent()
        {
            for (int i = 0;i<materialCount;i++)
            {
                if (behavoir[3 * numChem + i])
                    material = i;
            }
        }

    }
}
