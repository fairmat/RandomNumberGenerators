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
using DVPLI;
using Mono.Addins;

namespace RandomSourcesSupport.FileRandomSource
{
    /// <summary>
    /// Represent the file path and type to use as a random source.
    /// </summary>
    [SettingsContainer("File random source settings")]
    [Extension("/Fairmat/UserSettings")]
    [Serializable]
    public class FileRandomSourceSettings : ISettings
    {
        #region Fields
        /// <summary>
        /// The path of the file that acts as a random source.
        /// </summary>
        [PathSettingDescription("Path of the file random source")]
        public string FilePath;

        /// <summary>
        /// The type of the file that acts as a random source.
        /// </summary>
        [SettingDescription("Type of the file")]
        public FileType FileType;
        #endregion // Fields
    }
}
