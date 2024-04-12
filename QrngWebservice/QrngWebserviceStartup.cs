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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DVPLI;
using Mono.Addins;

namespace QrngWebservice
{
    /// <summary>
    /// Performs the startup operations for the plugin (for example checks what version
    /// of the library to use for connecting to the webservice).
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
            // Check the correct library to use
            LibQrngCheck();

            // Check if the credentials are null or empty
            QrngWebserviceSettings settings = UserSettings.GetSettings(typeof(QrngWebserviceSettings)) as QrngWebserviceSettings;
            if (string.IsNullOrEmpty(settings.Username) || string.IsNullOrEmpty(settings.Password.Value))
            {
                //MessageBox.Show(
                throw new ArgumentNullException(
                    "The credentials for accessing the webservice that retrieves the random numbers are empty." +
                    "\n\rIf you don't have the credentials you can subscribe freely at https://qrng.physik.hu-berlin.de" +
                    "\n\r\n\rYou can enter or change your credentials at any time in Fairmat Settings->Plugins settings");
                //, "QRNG");
            }
        }
        #endregion // ICommand implementation

        #region Helper methods
        /// <summary>
        /// Checks the right version on the libqrng library to use based on the underlying OS.
        /// </summary>
        private void LibQrngCheck()
        {
            string libOsDependantDll = "libqrng{0}{1}.qrng";
            string libDll = "libqrng.qrng";
            string platform, bit;

            // Check if we are running under Windows or under Unix
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
                platform = "Win";
            else
                platform = "Unix";

            // Check if the process is a 32 bit process or a 64 bit process
            if (System.Environment.Is64BitProcess)
                bit = "64";
            else
                bit = "32";

            // Rename the correct library to libqrng.dll if it's not already present
            libOsDependantDll = string.Format(libOsDependantDll, platform, bit);
            string pluginsPath = Path.GetFullPath(Assembly.GetAssembly(typeof(QrngWebserviceStartup)).Location);
            pluginsPath = Path.GetDirectoryName(pluginsPath);
            libDll = Path.Combine(pluginsPath, libDll);
            libOsDependantDll = Path.Combine(pluginsPath, libOsDependantDll);

            if (!File.Exists(libDll))
            {
                File.Copy(libOsDependantDll, libDll);

                // Delete the temporary dll files
                DirectoryInfo directoryInfo = new DirectoryInfo(pluginsPath);
                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.qrng"))
                {
                    if (Path.Combine(pluginsPath, fileInfo.Name) != libDll)
                        fileInfo.Delete();
                }
            }
        }
        #endregion // Fields
    }
}
