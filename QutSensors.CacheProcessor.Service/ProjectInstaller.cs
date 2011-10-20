// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the ProjectInstaller type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.CacheProcessor
{
    using System.ComponentModel;
    using System.Configuration.Install;

    /// <summary>
    /// Installer for CacheJobProcessor Service.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInstaller"/> class.
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
