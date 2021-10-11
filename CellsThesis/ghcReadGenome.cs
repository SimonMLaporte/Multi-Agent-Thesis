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
              "Thesis", "WIP")
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

            int numChem = genomes[0].chemNumber;
            int numMaterial = genomes[0].materialNumber;
            List<string> methods = new List<string> (){ " is smaller than ", " is smaller than ", " is larger than ", " is larger than ", " Always ", " Never "};
            List<string> variables = new List<string>() { "x", "y", "t" };
            List<string> text = new List<string>();
            
            for (int j =0;j<genomes.Count;j++)
            {
                string geneText = null;
                geneText = "Ruleset for Genome " + (j + 1).ToString() + System.Environment.NewLine;
                for (int i = 0; i < genomes[0].DNA.GetLength(0); i++)
                {
                    string action = null;
                    if (i < numChem)
                    {
                        action = " deposit chemical " + (i + 1).ToString();
                    } else if (i < 2*numChem)
                    {
                        action = " steer towards chemical " + (i + 1-numChem).ToString();
                    } else if (i < 3*numChem)
                    {
                        action = " steer away from chemical " + (i + 1 - 2*numChem).ToString();
                    } else if (i < 3*numChem+numMaterial)
                    {
                        action = " represent material " + (i + 1 - 3*numChem).ToString();
                    }  else
                    {
                        action = " stop";
                    }

                    geneText += "Gene " + i.ToString() + ":" + " If " + variables[genomes[j].DNA[i, 1]] + methods[genomes[j].DNA[i, 2]] + genomes[j].DNA[i, 0].ToString() + action + System.Environment.NewLine;

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