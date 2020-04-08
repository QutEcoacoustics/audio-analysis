// <copyright file="AssemblyInfo.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Acoustics.Shared;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(Meta.Name)]
[assembly: AssemblyDescription(Meta.Description)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Meta.GroupName)]
[assembly: AssemblyProduct(Meta.Name)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ffe49822-6c42-4211-87a9-e5dd162fb189")]

[assembly: InternalsVisibleTo("Acoustics.Test")]