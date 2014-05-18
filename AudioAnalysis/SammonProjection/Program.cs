using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SammonProjection
{
    using System.Drawing;

    using gfoidl.SammonProjection;

    public static class SammonProgram
    {

        public class Arguments
        {
        }

        public static void Dev(Arguments arguments)
        {
            Execute(arguments);
        }

        public static void Execute(Arguments arguments)
        {
           
            RunAnalysis("dummy");
        }

        static void RunAnalysis(object inputData)
        {
            
            //gfoidl.SammonProjection.SammonsProjection project = new SammonsProjection();


        //    SammonsProjection projection = new SammonsProjection(
        //_inputData,
        //2,
        //1000);
        //    projection.CreateMapping();

        //    // Create colors and labels - here a lazy version is shown, it should
        //    // be read from the data set in real applications ;)
        //    Color[] color = new Color[150];
        //    string[] labels = new string[150];
        //    for (int i = 0; i < 50; i++)
        //    {
        //        color[i] = Color.Red;
        //        labels[i] = "set";
        //    }
        //    for (int i = 50; i < 100; i++)
        //    {
        //        color[i] = Color.Green;
        //        labels[i] = "vers";
        //    }
        //    for (int i = 100; i < 150; i++)
        //    {
        //        color[i] = Color.Blue;
        //        labels[i] = "virg";
        //    }

        //    SammonsProjectionPostProcess processing = new SammonsProjectionPostProcess(
        //        projection);
        //    processing.PointSize = 4;
        //    processing.FontSize = 8;
        //    return processing.CreateImage(300, 300, labels, color);
        }
    }
}
