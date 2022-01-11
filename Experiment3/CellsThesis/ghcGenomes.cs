using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CellsThesis
{
    public class ghcGenomes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ghcGenomes()
          : base("Simulation Genomes", "Genome",
              "Creates a list of genomes for simulation",
              "Thesis", "Experiment 3")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Number of Chemicals", "ChemNumber", "Describes how many chemicals are used in the simulation", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Materials", "MatNumber", "Describes how many materials are used in the simulation", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Generation size", "GenSize", "The number of individuals in each generation of the evolutionary algorithm",GH_ParamAccess.item);
            pManager.AddIntegerParameter("Face Count", "FaceCount", "number of faces in starting metamesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Genomes", "Genomes", "Connect to simulation Genomes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int iChemNum = 1;
            int iMatNum = 1;
            int iGenSize = 1;
            int iFaceCount = 1;
            List<Genome> genomes = new List<Genome>();

            DA.GetData(0, ref iChemNum);
            DA.GetData(1, ref iMatNum);
            DA.GetData(2, ref iGenSize);
            DA.GetData(3, ref iFaceCount);

            Globals.faceCount = iFaceCount;
            for (int i = 0;i<iGenSize;i++)
            {
                genomes.Add(new Genome());
            }

            DA.SetDataList(0, genomes);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.IconGenome;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0D2218FB-731A-4D9D-A1A3-B5051403FF95"); }
        }
    }
}