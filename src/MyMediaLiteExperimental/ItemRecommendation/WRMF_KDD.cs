// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
// Copyright (C) 2011 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using MathNet.Numerics;
using MyMediaLite.DataType;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Weighted matrix factorization method proposed by Hu et al. and Pan et al.; adapted to KDD Cup 2011</summary>
	/// <remarks>
	/// </remarks>
	public sealed class WRMF_KDD : WRMF
	{
		/// <inheritdoc/>
		public override void Iterate()
		{
			// perform alternating parameter fitting
			Optimize(Feedback.UserMatrix, Feedback.ItemMatrix, user_factors, item_factors);
			Optimize(Feedback.ItemMatrix, Feedback.UserMatrix, item_factors, user_factors);
		}

		/// <summary>Optimizes the specified data</summary>
		/// <param name="data">data</param>
		/// <param name="inverse_data">data</param>
		/// <param name="W">W</param>
		/// <param name="H">H</param>
		void Optimize(IBooleanMatrix data, IBooleanMatrix inverse_data, Matrix<double> W, Matrix<double> H)
		{
			var HH          = new Matrix<double>(num_factors, num_factors);
			var HC_minus_IH = new Matrix<double>(num_factors, num_factors);
			var HCp         = new double[num_factors];

			var m = new MathNet.Numerics.LinearAlgebra.Matrix(num_factors, num_factors);
			MathNet.Numerics.LinearAlgebra.Matrix m_inv;
			// TODO speed up using more parts of that library

			// TODO using properties gives a 3-5% performance penalty

			// source code comments are in terms of computing the user factors
			// works the same with users and items exchanged

			// (1) create HH in O(f^2|Items|)
			// HH is symmetric
			for (int f_1 = 0; f_1 < num_factors; f_1++)
				for (int f_2 = 0; f_2 < num_factors; f_2++)
				{
					double d = 0;
					for (int i = 0; i < H.dim1; i++)
						d += H[i, f_1] * H[i, f_2];
					HH[f_1, f_2] = d;
				}
			// (2) optimize all U
			// HC_minus_IH is symmetric
			for (int u = 0; u < W.dim1; u++)
			{
				var row = data.GetEntriesByRow(u);

				// prepare KDD Cup specific weighting
				int num_user_items = row.Count;
				int user_positive_weight_sum = 0;
				foreach (int i in row)
					user_positive_weight_sum += inverse_data.NumEntriesByRow(i);
				double neg_weight_normalization = (double) (num_user_items * (1 + CPos)) / (Feedback.Count - user_positive_weight_sum);
				// TODO precompute
				// TODO check whether this is correct

				// create HC_minus_IH in O(f^2|S_u|)
				for (int f_1 = 0; f_1 < num_factors; f_1++)
					for (int f_2 = 0; f_2 < num_factors; f_2++)
					{
						double d = 0;
						foreach (int i in row)
							//d += H[i, f_1] * H[i, f_2] * (c_pos - 1);
							d += H[i, f_1] * H[i, f_2] * CPos;
						HC_minus_IH[f_1, f_2] = d;
					}
				// create HCp in O(f|S_u|)
				for (int f = 0; f < num_factors; f++)
				{
					double d = 0;
					for (int i = 0; i < inverse_data.NumberOfRows; i++)
						if (row.Contains(i))
							d += H[i, f] * (1 + CPos);
						else
							d += H[i, f] * inverse_data.NumEntriesByRow(i) * neg_weight_normalization;
					HCp[f] = d;
				}
				// create m = HH + HC_minus_IH + reg*I
				// m is symmetric
				// the inverse m_inv is symmetric
				for (int f_1 = 0; f_1 < num_factors; f_1++)
					for (int f_2 = 0; f_2 < num_factors; f_2++)
					{
						double d = HH[f_1, f_2] + HC_minus_IH[f_1, f_2];
						if (f_1 == f_2)
							d += Regularization;
						m[f_1, f_2] = d;
					}
				m_inv = m.Inverse();
				// write back optimal W
				for (int f = 0; f < num_factors; f++)
				{
					double d = 0;
					for (int f_2 = 0; f_2 < num_factors; f_2++)
						d += m_inv[f, f_2] * HCp[f_2];
					W[u, f] = d;
				}
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "WRMF_KDD num_factors={0} regularization={1} c_pos={2} num_iter={3} init_mean={4} init_stdev={5}",
								 NumFactors, Regularization, CPos, NumIter, InitMean, InitStdev);
		}
	}
}