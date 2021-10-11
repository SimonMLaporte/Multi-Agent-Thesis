using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Rhino.Geometry;
using System.Threading.Tasks;

namespace CellsThesis
{
    public class CellModel
    {
        List<Cell> cells;
        Environment environment;
        public int ID;
        public Random r;
        public int range;
        int t;

        // Class constructor
        public CellModel(int intNumber, Genome genome, Environment Environment)
        {
            t = 0;
            cells = new List<Cell>();
            ID = 1;
            range = 199;
            r = new Random();
            environment = Environment;
            CreateCells(intNumber, genome);

        }
        public void Update()
        {
            //Rhino.RhinoApp.WriteLine("New Step");

            //Globals.StartClock();

            // shuffle list to avoid emerginc properties (Fisher-Yates shuffle)
            Random rng = new Random();

            int n = cells.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Cell value = cells[k];
                cells[k] = cells[n];
                cells[n] = value;
            }

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Decide();
                //cells[i].Stop();
                cells[i].Represent();
            }

            //Rhino.RhinoApp.WriteLine("Other Cell Actions took " + Globals.StopClock());



            //Globals.StartClock();
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].run)
                    cells[i].Sense(environment);

            }
            //Rhino.RhinoApp.WriteLine("Sensing took " + Globals.StopClock());



            //Globals.StartClock();
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].run)
                    cells[i].Move(environment);

            }
            //Rhino.RhinoApp.WriteLine("Movement took " + Globals.StopClock()) ;


            //Globals.StartClock(); 
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].run)
                    cells[i].Deposit(environment);

            }

            //Rhino.RhinoApp.WriteLine("Deposit took " + Globals.StopClock());


            //Globals.StartClock();
            environment.Diffuse();
            //Rhino.RhinoApp.WriteLine("Diffuse took " + Globals.StopClock());

            t++;


        }
        public List<Point2d> GetPositions(int Material)
        {

            List<Point2d> curPositions = new List<Point2d>();
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].material == Material)
                    curPositions.Add(cells[i].positionF);

            }
            return curPositions;
        }



        public void CreateCells(int number, Genome genome)
        {
            // Initialize array with list of all positions
            List<int[]> emptyPositions = new List<int[]>();
            for (int i = 0; i < 200; i++)
                for (int j = 0; j < 200; j++)
                {
                    int[] x = new int[2];
                    x[0] = i;
                    x[1] = j;
                    emptyPositions.Add(x);
                }

            // Get random position
            Random rng = new Random();
            int n = number;
            while (n > 0)
            {
                int index = rng.Next(0, emptyPositions.Count);
                int[] randPos = emptyPositions[index];
                emptyPositions.RemoveAt(index);
                Point2d pos = new Point2d(randPos[0], randPos[1]);
                //Create Cell
                cells.Add(new Cell(pos, ID, genome));
                // Assign ID to ID lattice
                environment.cellLattice[randPos[0], randPos[1]] = ID;
                ID++;
                n--;
            }

        }


    }

}
