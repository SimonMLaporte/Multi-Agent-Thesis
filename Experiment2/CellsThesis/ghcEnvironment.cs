using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CellsThesis
{
    public class ghcEnvironment : GH_Component
    {

        public ghcEnvironment()
          : base("Simulation Environment", "Environment",
              "This component generates the environment to run simulation is",
              "Thesis", "Experiment 2")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Chemical Decay", "Decay", "Descripes how fast chemicals dissipates from the environment.Between 0 and 1. 1 is no dissipation", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Chemical Number", "ChemNum", "Descripes how many chemical layers are used in the sumulation", GH_ParamAccess.item);
            pManager.AddTextParameter("Chemical Bitmap location", "Bitmap Path", "Add a path to 200x200 bitmap, where chemical are stored in nuances of red", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Simulation Environment", "Env", "Connect to main solver environment input", GH_ParamAccess.item);
            pManager.AddPointParameter("Points of stimulants", "pt", "Each branch show a collection of points reporesenting the chemical stimulants from the respective chemical", GH_ParamAccess.tree);
        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int iNumChem = 1;
            double iDecay = 1;
            String iPath = null;

            DA.GetData(0, ref iDecay);
            DA.GetData(1, ref iNumChem);
            DA.GetData(2, ref iPath);

            Bitmap bitmap = new Bitmap(iPath);
            Environment environment = new Environment(iNumChem, bitmap, iDecay);
            DA.SetData(0, environment);

            // Send points from spawners to output
            DataTree<Point2d> FoodPoints = new DataTree<Point2d>();
            for (int i = 0; i < iNumChem; i++)
            { 
                List<Point2d> foodPoints = new List<Point2d>();
                for (int j = 0; j < 200; j++)
                {
                    for (int k = 0; k < 200; k++)
                    {
                        if (environment.chemSpawners[j, k, i]>10)
                        {
                            foodPoints.Add(new Point2d(j, k));
                        }
                        
                    }
                }
                FoodPoints.AddRange(foodPoints, new Grasshopper.Kernel.Data.GH_Path(i));
            }
            DA.SetDataTree(1, FoodPoints);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                
                return Properties.Resources.IconEnvironment;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("F9B9A404-F112-4E75-924D-477EEE1FF8AE"); }
        }
    }
}