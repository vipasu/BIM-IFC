//
// BIM IFC library: this library works with Autodesk(R) Revit(R) to export IFC files containing model geometry.
// Copyright (C) 2012  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;


namespace BIM.IFC.Utility
{
    /// <summary>
    /// Provides static methods for mathematical functions.
    /// </summary>
    class MathUtil
    {
        /// <summary>
        /// Returns a small value for use in comparing doubles.
        /// </summary>
        /// <returns>
        /// The value.
        /// </returns>
        public static double Eps()
        {
            return 1.0e-9;
        }

        /// <summary>
        /// Returns a small value for use in comparing angles.
        /// </summary>
        /// <returns>
        /// The value.
        /// </returns>
        public static double AngleEps()
        {
            return Math.PI / 1800.0;
        }

        /// <summary>
        /// Check if two double variables are almost equal.
        /// </summary>
        /// <returns>
        /// True if they are almost equal, false otherwise.
        /// </returns>
        public static bool IsAlmostEqual(double d1, double d2)
        {
            double sum = Math.Abs(d1) + Math.Abs(d2);
            if (sum < Eps())
                return true;
            return (Math.Abs(d1 - d2) <= sum * Eps());
        }

        /// <summary>
        /// Check if two UV variables are almost equal.
        /// </summary>
        /// <returns>
        /// True if they are almost equal, false otherwise.
        /// </returns>
        public static bool IsAlmostEqual(UV uv1, UV uv2)
        {
            return IsAlmostEqual(uv1.U, uv2.U) && IsAlmostEqual(uv1.V, uv2.V);
        }

        /// <summary>
        /// Check if the double variable is almost equal to zero.
        /// </summary>
        /// <returns>
        /// True if the value is almost zero, false otherwise.
        /// </returns>
        public static bool IsAlmostZero(double dd)
        {
            return Math.Abs(dd) <= Eps();
        }
    }
}