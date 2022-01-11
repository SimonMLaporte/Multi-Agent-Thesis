    using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CellsThesis
{
    public class ghcEvolution : GH_Component
    {

        public ghcEvolution()
          : base("Evolution Solver", "Evolution",
            "This Module calculates mutation and crossover",
            "Thesis", "Experiment 3")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Fitness for genomes", "Fitness", "Ordered list of fitness of genomes, indecies must correspont to genomes", GH_ParamAccess.list);
            pManager.AddNumberParameter("Probability for mutation", "Mutation Chance", "How likely is it that a gene is replaced by a random value?", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Elitism", "Elitism", "How many of the best performing individuals are directly passed to the next generation?", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("New Genomes", "New Genomes", "Genomes after crossover and mutation", GH_ParamAccess.list);
            pManager.AddGenericParameter("Best Genome", "Best Genome", "Genome with the highest fitness of the current generation", GH_ParamAccess.item);
            pManager.AddNumberParameter("Best Fitness", "Best fitness", "The fitness value of the fittest genome of the current generation", GH_ParamAccess.item);
        }



        Genome bestGenome;
        List<Genome> curGenome;
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<double> iFitness = new List<double>();
            double iMutRate = 0;
            int iElitism = 0;
            DA.GetDataList(0, iFitness);
            DA.GetData(1, ref iMutRate);
            DA.GetData(2, ref iElitism);

            //Make sure code doesnt run uninitialized
            if (Globals.simFinisehd == true & iFitness.Count == Globals.genomes.Count)
            {

                //Assign new fitness
                Rhino.RhinoApp.WriteLine("New Evolution simulation");
                for (int j = 0; j < Globals.genomes.Count; j++)
                    Globals.genomes[j].fitness = iFitness[j];

                // Run evolutionary simulation
                Globals.generation++;
                Globals.Selection(iElitism);
                Globals.Crossover();
                Globals.Mutate(iMutRate);
                Globals.ReplaceOldGenomes();
                curGenome = Globals.GetNewGenomes();

                bestGenome = Globals.bestIndividual;
                Rhino.RhinoApp.WriteLine("Best individual has fitness " + bestGenome.fitness.ToString());
                Rhino.RhinoApp.WriteLine("New Generation " + Globals.generation.ToString());
                Globals.simFinisehd = false;

            }
            DA.SetDataList(0, curGenome);
            DA.SetData(1, bestGenome);
            DA.SetData(2, bestGenome.fitness);


        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.IconEvolution;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7E8D3854-A852-4094-82FC-3E888981F7CB"); }
        }
    }
}