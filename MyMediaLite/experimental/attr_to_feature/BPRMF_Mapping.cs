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

using MyMediaLite;
using MyMediaLite.data_type;
using MyMediaLite.item_recommender;


namespace MyMediaLite.experimental.attr_to_feature
{
	// TODO: think again about composition instead of derivation
	// TODO: implement load/store
	// TODO: supports only binary attributes; implement also nominal/"integer" attributes

	public abstract class BPRMF_Mapping : BPRMF
	{
		public double learn_rate_mapping = 0.01;
		public int num_iter_mapping = 10;
		public int num_init_mapping = 5;   // TODO make it a parameter
		public double reg_mapping = 0.1;

		// includes bias
		protected Matrix<double> attribute_to_feature;

		public abstract void LearnAttributeToFactorMapping();
		public abstract void iterate_mapping();
	}
}
