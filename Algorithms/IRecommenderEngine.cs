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

namespace MyMediaLite
{
    /// <summary>
    /// Generic interface for simple recommender engines.
    /// </summary>
    /// <remarks>
    /// The ToString() method of recommender engines should list all hyperparameters, separated by space characters.
    /// </remarks>
    public interface IRecommenderEngine
    {
		/// <summary>
		/// Predict rating or score for a given user-item combination
		/// </summary>
		/// <param name="user_id">
		/// the user ID
		/// </param>
		/// <param name="item_id">
		/// the item ID
		/// </param>
		/// <returns>
		/// the predicted score/rating for the given user-item combination
		/// </returns>
        double Predict(int user_id, int item_id);

		/// <summary>
		/// Learn the model parameters of the engine from the training data
		/// </summary>
        void Train();

		/// <summary>
		/// Save the model parameters to a file
		/// </summary>
		/// <param name="filename">the name of the file to write to</param>
		void SaveModel(string filename);

		/// <summary>
		/// Get the model parameters from a file
		/// </summary>
		/// <param name="filename">the name of the file to read from</param>
		void LoadModel(string filename);
   }
}