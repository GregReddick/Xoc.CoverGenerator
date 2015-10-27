//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="PageType.cs" company="Xoc Software">
// Copyright © 2015 Xoc Software
// </copyright>
// <summary>Implements the page type enum</summary>
//------------------------------------------------------------------------------------------------------------------------------------------
namespace Xoc.CoverGenerator
{
	using System;

	/// <summary>
	/// Values that represent the CreateSpace page types. Each entry here must have a corresponding entry in the switch
	/// statement in PageThickness.
	/// </summary>
	[Serializable]
	public enum PageType
	{
		/// <summary>White pages.</summary>
		White,

		/// <summary>Cream pages.</summary>
		Cream,

		/// <summary>Color pages.</summary>
		Color
	}
}