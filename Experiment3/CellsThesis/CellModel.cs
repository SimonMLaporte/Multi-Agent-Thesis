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
        public static int cullOffset;
        List<Cell> cells;
        Environment environment;
        Genome genome;
        public int ID;
        public Random r;
        public int range;
        int t;
        public Mesh metaLayer;
        public int[] metaValues;
        public List<Point2d> metaCenterPoints = new List<Point2d>();
        int gMin;
        int gMax;
        int sMin;
        int sMax;
        double pDiv;

        // Class constructor
        public CellModel(int intNumber, Genome iGenome, Environment Environment, Mesh startMesh, int iCullOffset)
        {
            t = 0;
            cells = new List<Cell>();
            ID = 2;
            range = 199;
            r = new Random();
            environment = Environment;
            genome = iGenome;
            CreateCells(intNumber, iGenome);
            metaLayer = startMesh.DuplicateMesh();
            metaValues = new int[startMesh.Faces.Count];
            gMin = 0;
            gMax = 10;
            sMin = 0;
            sMax = 24;
            pDiv = 1.0;
            cullOffset = iCullOffset;
            // calculate centerpoints
            for (int i = 0; i < metaLayer.Faces.Count; i++)
                metaCenterPoints.Add(CentrePoint(i, metaLayer));
        }
        public void Update()
        {


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


            if (Globals.fixMesh == false)
                AdjustMetaLayer();


            environment.ClearBinLattice();
            Globals.faceSize = CalculateFaceAreas();
            Globals.StartClock();
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].BinLattice();
                //cells[i].DetermineRuleSet(metaCenterPoints);


            }
            //Rhino.RhinoApp.WriteLine("Determine ruleset tool " + Globals.StopClock());

            for (int i = 0; i < cells.Count; i++)
            {
                
                cells[i].Decide();
                //cells[i].Represent();
            }

            //Globals.StartClock();
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Sense(environment);
            }
            //Rhino.RhinoApp.WriteLine("Sensing took " + Globals.StopClock());



            //Globals.StartClock();
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Move(environment);
            }
            //Rhino.RhinoApp.WriteLine("Movement took " + Globals.StopClock()) ;


            //Globals.StartClock(); 
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Deposit(environment);
            }

            //Rhino.RhinoApp.WriteLine("Deposit took " + Globals.StopClock());



            
            if (t % 2 == 0)
            {
                //Globals.StartClock();
                environment.Diffuse();
                //Rhino.RhinoApp.WriteLine("Diffuse took " + Globals.StopClock());
            }


            //Globals.StartClock();
            //for (int i = 0; i < cells.Count; i++)
            //{
            //    metaValues = cells[i].MetaDeposit(metaValues);
            //}
            //Rhino.RhinoApp.WriteLine("MetaDeposit took " + Globals.StopClock());


            //for (int i = 0; i < cells.Count; i++)
            //{
            //    cells[i].Eat(environment);
            //}

            if (t % 3 == 0)
            {
                //Globals.StartClock();
               //environment.FindNeighbours(2); // these methods can only be called with an offset of 2 or 4
               //environment.FindNeighbours(4,0);
               //environment.FindNeighbours(4,1);
                //Rhino.RhinoApp.WriteLine("Finding neighbours took " + Globals.StopClock());
                //DivideCheck();
                //ShrinkCheck();

            }

            environment.CalculateInterfaceMap();

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
            List<int> cWeights = ghcCellSolver.iWeights;
            List<int> numberMats = new List<int>();
            for (int i = 0; i<Globals.chemNumber;i++)
            {
                numberMats.Add((int)(number * ((double)cWeights[i]/100.0)));
            }
           
            // Initialize array with list of all positions
            List<int[]> emptyPositions = new List<int[]>();

            for (int i = 0; i < 200; i++)
                for (int j = 60; j < 140; j++)
                {
                    if (environment.obstacleLattice[i,j] == 0)
                    {
                        int[] x = new int[2];
                        x[0] = i;
                        x[1] = j;
                        emptyPositions.Add(x);
                    }
                    
                }

            // Get random position
            
            for (int i = 0; i <Globals.chemNumber;i++)
            {
                while (numberMats[i] > 0)
                {
                    int index = Globals.r.Next(0, emptyPositions.Count);
                    int[] randPos = emptyPositions[index];
                    emptyPositions.RemoveAt(index);
                    Point2d pos = new Point2d(randPos[0], randPos[1]);
                    //Create Cell
                    cells.Add(new Cell(pos, ID, genome, i));
                    // Assign ID to ID lattice
                    environment.cellLattice[randPos[0], randPos[1], cells[cells.Count - 1].material] = ID;
                    ID++;
                    numberMats[i]--;
                }
            }
            

        }
        public void AdjustMetaLayer()
        {

            for (int i = 0; i < metaValues.Length; i++)
            {


                if (metaValues[i] > 1000) // what value should trigger this?
                {
                    metaValues[i] = 0; // GrowthValue is reset
                    Mesh newMesh = metaLayer;

                    for (int j = 0; j < metaLayer.Vertices.Count; j++)
                    {
                        double x = metaCenterPoints[i].X - metaLayer.Vertices[j].X;
                        double y = metaCenterPoints[i].Y - metaLayer.Vertices[j].Y;
                        Vector3f difference = new Vector3f((float)x, (float)y, 0);
                        double dist = Math.Max(difference.Length, 10); // ensure no strange jumps when points get too close
                        difference.Unitize();
                        difference = Vector3f.Multiply(difference, 200 / (float)dist); // Add Sccale Adjust Scale!!!!
                        difference.Reverse();
                        newMesh.Vertices[j] += difference;
                    }
                    metaLayer = newMesh;
                    metaCenterPoints[i] = CentrePoint(i, metaLayer);
                }
                else if (metaValues[i] < -1000)
                {
                    Mesh newMesh = metaLayer;
                    metaValues[i] = 0; // GrowthValue is reset

                    for (int j = 0; j < metaLayer.Vertices.Count; j++)
                    {
                        double x = metaCenterPoints[i].X - metaLayer.Vertices[j].X;
                        double y = metaCenterPoints[i].Y - metaLayer.Vertices[j].Y;
                        Vector3f difference = new Vector3f((float)x, (float)y, 0);
                        double dist = Math.Max(difference.Length, 10); // ensure no strange jumps when points get too close
                        difference.Unitize();
                        difference = Vector3f.Multiply(difference, 200 / (float)dist);

                        newMesh.Vertices[j] += difference;
                    }
                    metaLayer = newMesh;
                    metaCenterPoints[i] = CentrePoint(i, metaLayer);
                }

            }
        }

        void DivideCheck()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                int[] pos = cells[i].positionD;
                int neighbours = Environment.neighbourLattice9[pos[0], pos[1],cells[i].material];
                double value = cells[i].chemValues[0, 2];
                if (neighbours > gMin & neighbours <= gMax & value > 5)
                {

                    Vector2d newDir = new Vector2d(1, 0);
                    newDir.Rotate(Globals.r.NextDouble() * 6.28);
                    Point2d newPos = cells[i].positionF + newDir;
                    if (environment.cellLattice[(int)Math.Round(newPos.X), (int)Math.Round(newPos.Y),cells[i].material] == 0)
                    {
                        cells.Add(new Cell(newPos, ID, genome,cells[i].material));
                        environment.cellLattice[(int)Math.Round(newPos.X), (int)Math.Round(newPos.Y),cells[i].material] = 0;
                        ID++;
                    }
                }
            }
        }

        void ShrinkCheck()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                int[] pos = cells[i].positionD;
                int neighbours = Environment.neighbourLattice5[pos[0], pos[1],cells[i].material];
                if (neighbours < sMin || neighbours > sMax)
                {
                    environment.cellLattice[pos[0], pos[1],cells[i].material] = 0 ;
                    cells.RemoveAt(i);
                }
            }
        }

        public double[] CalculateFaceAreas()
        {
            double[] area = new double[metaLayer.Faces.Count];
            for (int i = 0; i < metaLayer.Faces.Count; i++)
            {
                // Get vertices of face as points
                Point3d[] pts = new Point3d[4];
                pts[0] = metaLayer.Vertices[metaLayer.Faces[i].A];
                pts[1] = metaLayer.Vertices[metaLayer.Faces[i].B];
                pts[2] = metaLayer.Vertices[metaLayer.Faces[i].C];
                pts[3] = metaLayer.Vertices[metaLayer.Faces[i].D];

                //calculate triangleareas
                double a = pts[0].DistanceTo(pts[1]);
                double b = pts[1].DistanceTo(pts[2]);
                double c = pts[2].DistanceTo(pts[0]);
                double p = 0.5 * (a + b + c);
                double area1 = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

                a = pts[0].DistanceTo(pts[2]);
                b = pts[2].DistanceTo(pts[3]);
                c = pts[3].DistanceTo(pts[0]);
                p = 0.5 * (a + b + c);
                double area2 = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

                area[i] = area1 + area2;
            }


            return area;
        }

        Point2d CentrePoint(int meshfaceindex, Mesh m)
        {
            var temppt = new Point2d(0, 0);

            temppt.X += m.Vertices[m.Faces[meshfaceindex].A].X;
            temppt.Y += m.Vertices[m.Faces[meshfaceindex].A].Y;


            temppt.X += m.Vertices[m.Faces[meshfaceindex].B].X;
            temppt.Y += m.Vertices[m.Faces[meshfaceindex].B].Y;


            temppt.X += m.Vertices[m.Faces[meshfaceindex].C].X;
            temppt.Y += m.Vertices[m.Faces[meshfaceindex].C].Y;


            if (m.Faces[meshfaceindex].IsQuad)
            {
                temppt.X += m.Vertices[m.Faces[meshfaceindex].D].X;
                temppt.Y += m.Vertices[m.Faces[meshfaceindex].D].Y;


                temppt.X /= 4;
                temppt.Y /= 4;

            }
            else
            {
                temppt.X /= 3;
                temppt.Y /= 3;

            }

            return temppt;
        }


    }

}
