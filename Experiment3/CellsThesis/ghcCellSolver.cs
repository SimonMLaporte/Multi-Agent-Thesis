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
            "Thesis", "Experiment 3")
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
            pManager.AddMeshParameter("Start Rule Mesh", "Start Mesh", "Starting mesh for ruledevision, each cell has a different ruleset for each material. Mesh will be changed for each genome at each generation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Next Genome", "Skip Genome", "Skips current genome and sends the current cell positions to fitness evaluation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Send Chemicals", "Send Chemicals", "should chemicals be send to outout WARNING: may slow down algorithm significantly", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Master Culling Offset", "Cull Offset", "Distance that the master material cull the slave chemicals", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Material Fractions", "Material Fractions", "List of percentage material for each discipline. The list should add up to 100%, the indicies of the list correspond directly with the disciplines", GH_ParamAccess.list);
            
            
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
            pManager.AddMeshParameter("Current RuleMesh", "RuleMesh", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("current face values", "Face Values", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Collusion count in the interface map", "Interface Map", "it counts every time a cell is occupied by both materials, The data is structured as a list with all values in a row combined, one branch for each discipline interaction (0 is human-struct, 1 is struct-hvac, 2 is hvac-human" , GH_ParamAccess.tree);

        }



        //Variables to keep
        int GenomeNumber;
        List<Genome> iGenomes;
        public static List<int> iWeights;
        CellModel cellModel;
        private DataTree<Point2d> Solutions;
        Environment iEnvironment;
        int t;
        bool iSendChems;
        int iMaxStep = 1000;
        int iNumber = 1000;
        Mesh iStartMesh = null;
        int iCullOffset;
        // Main solver
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Only get triggers, constants are not assigned each timestep
            bool iReset = true;
            bool nextGenome = false;
            DA.GetData(1, ref iReset);
            DA.GetData(6, ref nextGenome);
            DA.GetData(7, ref iSendChems);
            // Initialize
            if (iReset || cellModel == null)
            {
                Rhino.RhinoApp.WriteLine("Initialize Simulation");
                iGenomes = new List<Genome>();
                iWeights = new List<int>();
                DA.GetData(0, ref iMaxStep);
                DA.GetData(2, ref iNumber);
                DA.GetDataList(3, iGenomes);
                DA.GetDataList(9, iWeights);
                DA.GetData(4, ref iEnvironment);
                DA.GetData(5, ref iStartMesh);
                DA.GetData(8, ref iCullOffset);
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
                //skip current genome
                if (nextGenome)
                {
                    Rhino.RhinoApp.WriteLine("Skip current genome" + " skip to t=" + iMaxStep);
                    t = iMaxStep-1;
                }
                    
                // Initialize cellModel
                if (t == 0)
                { cellModel = new CellModel(iNumber, Globals.genomes[GenomeNumber], iEnvironment, iStartMesh,iCullOffset); }
                

                

                //Update model
                cellModel.Update();


                DataTree<double> chemicals = new DataTree<double>();

                // Other Actions
                //Globals.StartClock();
                // Loading of chemicals is removed to speed up algorithm
                if (iSendChems == true)
                {
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
                }
                




                //Send current positions to outpu
                DataTree<Point2d> allcells = new DataTree<Point2d>();
                for (int i = 0; i < Globals.materialNumber; i++)
                {
                    allcells.AddRange(cellModel.GetPositions(i), new Grasshopper.Kernel.Data.GH_Path(i));
                }

                t++;
                DA.SetDataTree(0, allcells);
                DA.SetData(2, t);
                if (iSendChems == true)
                {
                    DA.SetDataTree(3, chemicals);
                }
                
                DA.SetData(5, GenomeNumber);
                DA.SetData(6, Globals.generation);
                DA.SetDataList(7,Globals.genomes);
                DA.SetData(8, cellModel.metaLayer);
                DA.SetDataList(9, cellModel.metaValues);
                if (t % 10 == 0)
                    DA.SetDataTree(10, iEnvironment.interfaceMap);

                
                
               
                //Rhino.RhinoApp.WriteLine("Other Model Actions took " + Globals.StopClock());

                // Terminate simulation / load next model
                if (t == iMaxStep)
                {
                    iEnvironment.ClearLattice();
                    // Only send solutions to output after everything is calculated
                    for (int i = 0; i < Globals.materialNumber; i++)
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