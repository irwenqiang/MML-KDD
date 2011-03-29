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
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Matrix factorization engine with explicit user and item bias that takes item relations into account</summary>
	public class SharedFactorsMatrixFactorization : BiasedMatrixFactorization, IKDDCupRecommender
	{
		Matrix<double> user_shared_artist_factors;
		Matrix<double> user_shared_record_factors;
		Matrix<double> user_shared_genre_factors;

		Matrix<double> item_shared_artist_factors;
		Matrix<double> item_shared_record_factors;
		Matrix<double> item_shared_genre_factors;

		/// <summary>Number of shared factors for common artist</summary>
		public int NumSharedArtistFactors { get; set; }
		/// <summary>Number of shared factors for common records</summary>
		public int NumSharedRecordFactors { get; set; }
		/// <summary>Number of shared factors for common genres</summary>
		public int NumSharedGenreFactors { get; set; }

		/// <inheritdoc/>
		public KDDCupItems ItemInfo { get; set; }

		/// <inheritdoc/>
		protected override void InitModel()
		{
			base.InitModel();
		}

		/// <inheritdoc/>
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			double rating_range_size = MaxRating - MinRating;

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double dot_product = global_bias + user_bias[u] + item_bias[i];
				for (int f = 0; f < NumFactors; f++)
					dot_product += user_factors[u, f] * item_factors[i, f];
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));

				double p = MinRating + sig_dot * rating_range_size;
				double err = ratings[index] - p;

				double gradient_common = err * sig_dot * (1 - sig_dot) * rating_range_size;

				// Adjust biases
				if (update_user)
					user_bias[u] += LearnRate * (gradient_common - BiasRegularization * user_bias[u]);
				if (update_item)
					item_bias[i] += LearnRate * (gradient_common - BiasRegularization * item_bias[i]);

				// Adjust latent factors
				for (int f = 0; f < NumFactors; f++)
				{
				 	double u_f = user_factors[u, f];
					double i_f = item_factors[i, f];

					if (update_user)
					{
						double delta_u = gradient_common * i_f - Regularization * u_f;
						MatrixUtils.Inc(user_factors, u, f, LearnRate * delta_u);
					}
					if (update_item)
					{
						double delta_i = gradient_common * u_f - Regularization * i_f;
						MatrixUtils.Inc(item_factors, i, f, LearnRate * delta_i);
					}
				}
			}
		}

		/// <inheritdoc/>
		public override double Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1 || item_id >= item_factors.dim1)
				return MinRating + ( 1 / (1 + Math.Exp(-global_bias)) ) * (MaxRating - MinRating);

			double score = global_bias + user_bias[user_id] + item_bias[item_id];

			// U*V
			for (int f = 0; f < NumFactors; f++)
				score += user_factors[user_id, f] * item_factors[item_id, f];

			return MinRating + ( 1 / (1 + Math.Exp(-score)) ) * (MaxRating - MinRating);
		}

		/// <inheritdoc/>
		public override void SaveModel(string filename)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void LoadModel(string filename)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void AddUser(int user_id)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void AddItem(int item_id)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void RetrainUser(int user_id)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void RetrainItem(int item_id)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void RemoveUser(int user_id)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void RemoveItem(int item_id)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni,
								 "SharedFactorsMatrixFactorization NumFactors={0} bias_regularization={1} regularization={2} LearnRate={3} num_iter={4} init_mean={5} init_stdev={6}",
								 NumFactors, BiasRegularization, Regularization, LearnRate, NumIter, InitMean, InitStdev);
		}
	}
}