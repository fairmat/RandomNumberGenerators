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
using System.IO;
using System.Reflection;
using DVPLI;
using Mono.Addins;

namespace RandomSourcesSupport
{
    /// <summary>
    /// Performs the startup operations for the plugin.
    /// </summary>
    [Extension("/Fairmat/StartupOperation")]
    public class QrngWebserviceStartup : ICommand
    {
        #region ICommand implementation
        /// <summary>
        /// Performs the startup operations for the plugin.
        /// </summary>
        public void Execute()
        {
            SetListItems();
        }
        #endregion // ICommand implementation

        #region Helper methods
        /// <summary>
        /// Sets the items to show on the list of choices base on the random sources installed
        /// on the system.
        /// </summary>
        private void SetListItems()
        {
            SettingDescriptionAttribute attribute = SettingsAttributeUtility.GetSettingsAttribute(typeof(RandomSourceSettings), "RngRandomSource");
            ListSettingDescriptionAttribute attributeAsList = attribute as ListSettingDescriptionAttribute;
            if (attributeAsList != null)
            {
                attributeAsList.Clear(typeof(RandomSourceSettings), "RngRandomSource");
                foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes("/RandomSourcesSupport/RandomSource"))
                {
                    IRandomSource randomSource = node.CreateInstance() as IRandomSource;
                    if (randomSource != null)
                    {
                        string description = randomSource is IDescription ? ((IDescription)randomSource).Description : randomSource.ToString();
                        attributeAsList.AddItem(typeof(RandomSourceSettings), "RngRandomSource", description);
                    }
                }
            }
        }
        #endregion // Helper methods
    }
}
