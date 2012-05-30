/* Copyright (C) 2012 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
 * Author(s): Francesco Biondi (francesco.biondi@fairmat.com)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Runtime.Serialization;
using DVPLI;
using Mono.Addins;

namespace RandomSourcesSupport
{
    /// <summary>
    /// Represents the source to use in in order to retrieve the randomly generated numbers.
    /// </summary>
    [SettingsContainer("Random sources settings")]
    [Extension("/Fairmat/UserSettings")]
    [Serializable]
    public class RandomSourceSettings : ISettings
    {
        #region Fields
        /// <summary>
        /// The source to use in order to retrieve/store/load the randomly generated numbers.
        /// </summary>
        [ListSettingDescription("Rng random source", new object[] { "" })]
        public string RngRandomSource;
        #endregion // Fields
    }
}
