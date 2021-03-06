//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Xoc Software">
// Copyright © 2015 Xoc Software
// </copyright>
// <summary>Implements the global suppressions class</summary>
//------------------------------------------------------------------------------------------------------------------------------------------
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
	"Microsoft.Design",
	"CA1020:AvoidNamespacesWithFewTypes",
	Scope = "namespace",
	Target = "Xoc.Penrose",
	Justification = "Only item in library")]