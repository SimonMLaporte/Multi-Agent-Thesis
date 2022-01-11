using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CellsThesis
{
    public class ghcReadGenome : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ghcReadGenome()
          : base("Genome Reader", "ReadGenome",
              "This Component translates genomes into text",
              "Thesis", "Experiment 3")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Genomes", "Genomes", "Genomes to translate", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Translation", "Translation", "Plaintext of the content of the genome", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Genome> genomes = new List<Genome>(); ;
            DA.GetDataList(0, genomes);


            List<string> methods = new List<string> (){ " is smaller than ", " is larger than ", " Always ", " Never "};
            List<string> variables = new List<string>() { "chem value 1", "chem value 2", "chem value 3", "t","size 1","size 2","face size" };
            List<string> actions = new List<string>() { " deposit chemical ", " steer towards material chemical ", " steer towards other chemical ", " steer away from other chemical ", " switch material ", " grow metalattice ", " shrink metalattice ", " cull chemical ", " cull other chemical ", " set sensory offset to 12 ", " set sensory offset to 6 ", " die " };
            List<string> text = new List<string>();
            
            for (int p =0;p<genomes.Count;p++)
            {
                string geneText = null;
                geneText = "Ruleset for Genome " + (p + 1).ToString() + System.Environment.NewLine;
                for (int j = 0; j<Globals.faceCount;j++)
                    for(int k = 0; k<Globals.materialNumber;k++)
                    {

                        geneText += "Facenumber " + j.ToString() + " Material number " + k.ToString() + System.Environment.NewLine;
                        for (int i = 0; i < genomes[0].DNA.GetLength(0); i++)
                        {

                            geneText += "Gene " + i.ToString() + ":" + " If " + variables[genomes[p].DNA[i, 1,j,k]] + methods[genomes[p].DNA[i, 2,j,k]] + genomes[p].DNA[i, 0,j,k].ToString() + actions[i] + System.Environment.NewLine;
                        }

                    }

                
                text.Add(geneText);
            }


            DA.SetDataList(0, text);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FA96B12A-1F03-4379-A088-1C268FEF023A"); }
        }
    }
}