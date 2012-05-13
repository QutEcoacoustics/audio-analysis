using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace AnalysisRunner
{
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Registration;

    using AnalysisBase;

    public class Program
    {
        public static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
        }

        public void Run()
        {
            this.Compose();
        }

        private void Compose()
        {
            var registration = new RegistrationBuilder();
            registration.ForTypesDerivedFrom<IAnalysis>().Export<IAnalysis>();
            registration.ForTypesDerivedFrom<ISourcePreparer>().Export<ISourcePreparer>();
            var directoryCatalog = new DirectoryCatalog("AnalysisPlugins", registration);

            var assemblyCatalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());

            var aggregateCatlog = new AggregateCatalog(directoryCatalog, assemblyCatalog);

            var container = new CompositionContainer(aggregateCatlog);
            container.ComposeParts(this);
        }
    }
}
