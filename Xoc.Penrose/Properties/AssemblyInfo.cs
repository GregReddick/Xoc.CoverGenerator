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
[assembly: AssemblyDescription("Creates a Penrose Tiling Bitmap.")]
[assembly: AssemblyInformationalVersion("1.0")]
[assembly: AssemblyProduct("Xoc.Penrose")]
[assembly: AssemblyTitle("Xoc Penrose Tiler")]
[assembly: AssemblyTrademark("Xoc is a trademark of Xoc Software")]
[assembly: AssemblyVersion("1.0.*")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: ContractVerification(true)]
[assembly: Guid("22909ce5-e3e1-441e-ac8e-be93e8af89b6")]
[assembly: NeutralResourcesLanguage("en-US")]