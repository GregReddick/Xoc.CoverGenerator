//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Xoc Software">
// Copyright © 2015 Xoc Software
// </copyright>
// <summary>Implements the assembly information class</summary>
//------------------------------------------------------------------------------------------------------------------------------------------
using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Xoc Software")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCopyright("Copyright © 2015 Xoc Software")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyDescription("Generates cover for the book.")]
[assembly: AssemblyInformationalVersion("1.0")]
[assembly: AssemblyProduct("Xoc.CoverGenerator")]
[assembly: AssemblyTitle("Xoc Cover Generator")]
[assembly: AssemblyTrademark("Xoc is a trademark of Xoc Software")]
[assembly: AssemblyVersion("1.0.*")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: ContractVerification(true)]
[assembly: Guid("88b8a824-3272-4bf4-a0d4-ff74a032b11b")]
[assembly: NeutralResourcesLanguage("en-US")]

/// <summary>Gives information about the assembly.</summary>
internal static class AssemblyInfo
{
	/// <summary>Gets an assembly attribute.</summary>
	/// <typeparam name="T">An assembly attribute type.</typeparam>
	/// <returns>The assembly attribute of type T.</returns>
	internal static T Attribute<T>()
			where T : Attribute
	{
		return typeof(AssemblyInfo).Assembly.GetCustomAttribute<T>();
	}
}