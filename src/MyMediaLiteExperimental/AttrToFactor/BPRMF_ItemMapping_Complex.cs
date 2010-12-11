// Copyright (C) 2010 Zeno Gantner
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
using System.Diagnostics;
using System.Globalization;
using MyMediaLite;
using MyMediaLite.DataType;
using MyMediaLite.ItemRecommender;


namespace MyMediaLite.AttrToFactor
{
	public class BPRMF_ItemMapping_Complex : BPRMF_ItemMapping
	{
		public int num_hidden_factors = 80;

		protected Matrix<double> output_layer;

		public override void LearnAttributeToFactorMapping()
		{
			attribute_to_factor = new Matrix<double>(NumItemAttributes, num_hidden_factors); // TODO change name
			output_layer        = new Matrix<double>(num_hidden_factors, num_factors);

			Console.Error.WriteLine("BPR-MULTILAYER-MAP training");
			Console.Error.WriteLine("num_item_attributes=" + NumItemAttributes);
			Console.Error.WriteLine("num_hidden_factors=" + num_hidden_factors);

			MatrixUtils.InitNormal(attribute_to_factor, init_mean, init_stdev);
			MatrixUtils.InitNormal(output_layer, init_mean, init_stdev);

			//Console.Error.WriteLine("iteration -1 fit {0,0:0.#####} ", ComputeFit());
			for (int i = 0; i < num_iter_mapping; i++)
			{
				IterateMapping();
				//ComputeMappingFit();
				//Console.Error.WriteLine("iteration {0} fit {1,0:0.#####} ", i, ComputeFit());
			}

			// set item_factors to the mapped ones:                     // TODO: put into a separate method
			for (int item_id = 0; item_id < MaxItemID + 1; item_id++)
			{
				HashSet<int> attributes = item_attributes[item_id];

				// only map factors for items where we know attributes
				if (attributes.Count == 0)
					continue;

				double[] est_factors = MapToLatentFactorSpace(item_id);

				item_factors.SetRow(item_id, est_factors);
			}
		}

		/// <inheritdoc/>
		public override void IterateMapping()
		{
			Console.Error.Write(".");

			int num_pos_events = data_user.NumberOfEntries;

			for (int i = 0; i < num_pos_events / 50; i++)
			{
				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);
				UpdateMappingFactors(user_id, item_id_1, item_id_2);
			}
		}

		// TODO ADD IN THRESHOLD FUNCTION!!
		protected virtual void UpdateMappingFactors(int u, int i, int j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			HashSet<int> attr_i = item_attributes[i];
			HashSet<int> attr_j = item_attributes[j];

			// assumption: attributes are sparse
			var attr_i_over_j = new HashSet<int>(attr_i);
			attr_i_over_j.ExceptWith(attr_j);
			var attr_j_over_i = new HashSet<int>(attr_j);
			attr_j_over_i.ExceptWith(attr_i);

			// update hidden layer - m1_AB
			for (int b = 0; b < num_hidden_factors; b++)
			{
				foreach (int a in attr_i_over_j)
				{
					double sum = 0;
					for (int f = 0; f < num_factors; f++)
					{
						double w_uf = user_factors[u, f];
						double m2_bf = output_layer[b, f];
						sum += w_uf * m2_bf;
					}
					double m1_ab = attribute_to_factor[a, b];
					double update = sum / (1 + Math.Exp(x_uij)) - reg_mapping * m1_ab;
					attribute_to_factor[a, b] = m1_ab + learn_rate_mapping * update;
				}

				foreach (int a in attr_j_over_i)
				{
					double sum = 0;
					for (int f = 0; f < num_factors; f++)
					{
						double w_uf = user_factors[u, f];
						double m2_bf = output_layer[b, f];
						sum -= w_uf * m2_bf;
					}
					double m1_ab = attribute_to_factor[a, b];
					double update = sum / (1 + Math.Exp(x_uij)) - reg_mapping * m1_ab;
					attribute_to_factor[a, b] = m1_ab + learn_rate_mapping * update;
				}
			}

			// update output layer - m2_BF
			for (int b = 0; b < num_hidden_factors; b++)
			{
				for (int f = 0; f < num_factors; f++)
				{
					double w_uf = user_factors[u, f];

					double sum = 0;
					foreach (int a in attr_i_over_j)
					{
						double m1_ab = attribute_to_factor[a, b];
						sum += w_uf * m1_ab;
					}
					foreach (int a in attr_j_over_i)
					{
						double m1_ab = attribute_to_factor[a, b];
						sum -= w_uf * m1_ab;
					}
					double m2_bf = output_layer[b, f];
					double update = sum / (1 + Math.Exp(x_uij)) - reg_mapping * m2_bf;
					output_layer[b, f] = m2_bf + learn_rate_mapping * update;
				}
			}
		}

		protected override double[] MapToLatentFactorSpace(int item_id)
		{
			HashSet<int> attributes = this.item_attributes[item_id];
			var factor_representation = new double[num_factors];

			foreach (int i in attributes)
				for (int j = 0; j < num_factors; j++)
					for (int k = 0; k < num_hidden_factors; k++)
						factor_representation[j] += attribute_to_factor[i, k] * output_layer[k, j];
			// TODO ADD IN THRESHOLD FUNCTION

			return factor_representation;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(
				ni,
				"BPR-MF-ItemMapping-Complex num_factors={0}, reg_u={1}, reg_i={2}, reg_j={3}, num_iter={4}, learn_rate={5}, reg_mapping={6}, num_iter_mapping={7}, learn_rate_mapping={8}, num_hidden_factors={9}, init_mean={9}, init_stdev={10}",
				num_factors, reg_u, reg_i, reg_j, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, num_hidden_factors, init_mean, init_stdev
			);
		}

	}
}
