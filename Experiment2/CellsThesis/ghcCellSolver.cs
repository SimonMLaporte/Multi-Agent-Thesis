using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace CellsThesis
{
    public class ghcCellSolver : GH_Component
    {
        // Component information
        public ghcCellSolver()
          : base("CellSolver", "CellSolver",
            "This Module runs the agent simulation",
            "Thesis", "Experiment 2")
        {
        }

        // Inputs
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Steps", "tMax", "number of steps per simulation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset Simulation", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nCells", "Number of starting cells", GH_ParamAccess.item);
            pManager.AddGenericParameter("Genomes", "Geno", "Genomes to optimize", GH_ParamAccess.list);
            pManager.AddGenericParameter("Environment", "Env", "Environment to run Simulation in", GH_ParamAccess.item);
        }

        //Outputs
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "Positions", "CurrentPositions of simulation", GH_ParamAccess.list);
            pManager.AddPointParameter("Solutions", "Solutions", "Tree of solutions from simulation", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Current Step", "Current Step", "Current step of current simulation", GH_ParamAccess.item);
            pManager.AddNumberParameter("Chemical Layers", "Chems", "Each branch contains the chemical values for all positions in the 200x200 grid", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Model", "EvolutionModel", "Connect this output to evolutionart component model", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Current genome ID", "Current Genome ID", "Index of current genome being simulated", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Current generation", "Current Generation", "Current Generation of simulation", GH_ParamAccess.item);
            pManager.AddGenericParameter("All Genomes", "All Genomes", "All Genomes of current evolutionary step", GH_ParamAccess.list);
        }



        //Variables to keep
        int GenomeNumber;
        List<Genome> iGenomes;
        CellModel cellModel;
        private DataTree<Point2d> Solutions;
        Environment iEnvironment;
        int t;

        // Main solver
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Assign Variables
            bool iReset = true;
            int iMaxStep = 1000;
            int iNumber = 1000;
            iGenomes = new List<Genome>();
            DA.GetData(0, ref iMaxStep);
            DA.GetData(1, ref iReset);
            DA.GetData(2, ref iNumber);
            DA.GetDataList(3, iGenomes);



            // Initialize
            if (iReset || cellModel == null)
            {
                Rhino.RhinoApp.WriteLine("Initialize Simulation");
                
                DA.GetData(4, ref iEnvironment);
                t = 0;
                GenomeNumber = 0;
                Solutions = new DataTree<Point2d>();
                iEnvironment.ClearLattice(); // remove old CELL ids and clear chemicals

                Globals.InitClock();
                Globals.simFinisehd = false;
                Globals.genomes = iGenomes;
                Globals.generation = 0;

            }
            if (Globals.simFinisehd == false)
            {
                // Initialize cellModel
                if (t == 0)
                    cellModel = new CellModel(iNumber, Globals.genomes[GenomeNumber],iEnvironment);
               
                //Update model
                cellModel.Update();




                // Other Actions
                //Globals.StartClock();
                // Loading of chemicals is removed to speed up algorithm
                /*
                DataTree<double> chemicals = new DataTree<double>();
                // Construct dataTree of chemical values
                for (int i = 0; i < iEnvironment.numChem; i++)
                {
                    List<double> values = new List<double>();
                    for (int j = 0; j < 200; j++)
                        for (int k = 0; k < 200; k++)
                        {
                            values.Add(iEnvironment.GetValue(i, new Point2d(j, k)));
                        }

                    Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(i);
                    chemicals.AddRange(values, path);
                }
                */

                //Rhino.RhinoApp.WriteLine("Other Model Actions took " + Globals.StopClock());

                // Send current positions to outpup

                DataTree<Point2d> allcells = new DataTree<Point2d>();
                for (int i = 0; i < iGenomes[0].materialNumber; i++)
                {
                    allcells.AddRange(cellModel.GetPositions(i),new Grasshopper.Kernel.Data.GH_Path(i));
                }
                
                t++;
                DA.SetDataTree(0, allcells);
                DA.SetData(2, t);
                //DA.SetDataTree(3, chemicals);
                DA.SetData(5, GenomeNumber);
                DA.SetData(6, Globals.generation);
                DA.SetDataList(7,Globals.genomes);

                

                // Terminate simulation / load next model
                if (t == iMaxStep)
                {
                    iEnvironment.ClearLattice();
                    // Only send solutions to output after everything is calculated
                    for (int i = 0; i < iGenomes[0].materialNumber; i++)
                    {
                        Solutions.AddRange(cellModel.GetPositions(i), new Grasshopper.Kernel.Data.GH_Path(GenomeNumber, i));
                    }

                    if (GenomeNumber < Globals.genomes.Count - 1)

                    {
                        GenomeNumber++;
                        t = 0;
                        
                    }
                    else
                    {
                        GenomeNumber = 0;
                        t = 0;

                        Globals.simFinisehd = true;
                        Rhino.RhinoApp.WriteLine("Simulation is Over -> Calculate fitness");
                        
                        DA.SetDataTree(1, Solutions);


                    }
                }
            }

        }

        // Component Icon
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.IconCells;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6270F459-A9CA-421D-851B-58ED0850E260");
    }
}