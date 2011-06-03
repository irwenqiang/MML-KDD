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
using System.Linq;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>BPRMF with frequency-adjusted sampling, prototype for KDD Cup 2011</summary>
	public class BPRMF_KDD : BPRMF
	{
		// TODO offer this data structure from Feedback
		/// <summary>array of user IDs of positive user-item pairs</summary>
		protected int[] users;
		/// <summary>array of item IDs of positive user-item pairs</summary>
		protected int[] items;

		/// <summary>Default constructor</summary>
		public BPRMF_KDD()
		{
		}

		///
		public override void Train()
		{
			// prepare helper data structures for training
			users = new int[Feedback.Count];
			items = new int[Feedback.Count];

			int index = 0;
			foreach (int user_id in Feedback.UserMatrix.NonEmptyRowIDs)
				foreach (int item_id in Feedback.UserMatrix[user_id])
				{
					users[index] = user_id;
					items[index] = item_id;

					index++;
				}

			// suppress using user_neg_items in BPRMF
			FastSamplingMemoryLimit = 0;

			base.Train();
		}

		///
		protected override void SampleTriple(out int u, out int i, out int j)
		{
			// sample user from positive user-item pairs
			int index = random.Next(0, items.Length - 1);
			u = users[index];
			i = items[index];

			// sample negative item
			do
				j = items[random.Next(0, items.Length - 1)];
			while (Feedback.UserMatrix[u, j]);
		}

		///
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni, "BPRMF_KDD num_factors={0} bias_reg={1} reg_u={2} reg_i={3} reg_j={4} num_iter={5} bold_driver={6} learn_rate={7} init_mean={8} init_stdev={8}",
								 num_factors, BiasReg, reg_u, reg_i, reg_j, NumIter, BoldDriver, learn_rate, InitMean, InitStdev);
		}
	}
}

