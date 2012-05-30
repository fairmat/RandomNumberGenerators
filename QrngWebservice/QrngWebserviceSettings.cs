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

namespace QrngWebservice
{
    /// <summary>
    /// Represents the user credential for accessing the webservice
    /// (qrng.physik.hu-berlin.de) used to retrieve the generated random numbers.
    /// </summary>
    [SettingsContainer("qrng.physik.hu-berlin.de Settings")]
    [Extension("/Fairmat/UserSettings")]
    [Serializable]
    public class QrngWebserviceSettings : ISettings
    {
        #region Fields
        /// <summary>
        /// The username for the webservice.
        /// </summary>
        [SettingDescription("https://qrng.physik.hu-berlin.de/ username")]
        public string Username = string.Empty;

        /// <summary>
        /// The password for the webservice.
        /// </summary>
        [SettingDescription("https://qrng.physik.hu-berlin.de/ password")]
        public Password Password = new Password(string.Empty);
        #endregion // Fields
    }
}
