using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace CellsThesis
{
    public class CellsThesisInfo : GH_AssemblyInfo
    {
        public override string Name => "CellsThesis";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("F765C691-6B4D-4A58-A95C-9F4F999A0EFA");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}