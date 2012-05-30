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
using System.Runtime.InteropServices;

namespace QrngWebservice
{
    /// <summary>
    /// Defines the conversion between the methods of the unmanaged library to the managed one.
    /// </summary>
    public class LibQrng
    {
        #region Properties
        /// <summary>
        /// Gets the code returned in case of success by the unmanaged library.
        /// </summary>
        public static int SuccessReturnCode
        {
            get { return 0; }
        }
        #endregion // Properties

        /// <summary>
        /// Connects to the webservice using the given username and password.
        /// This is a call to the dll providing the service.
        /// </summary>
        /// <param name="username">The username for the webservice.</param>
        /// <param name="password">The password for the websevice.</param>
        /// <returns>The return code for the connect (the int SuccessReturnCode is returned in
        /// case of success).</returns>
        [DllImport("libqrng.qrng")]
        private static extern int qrng_connect(string username, string password);

        /// <summary>
        /// Fills an array of double with random numbers received from the webservice.
        /// This is a call to the dll providing the service.
        /// </summary>
        /// <param name="array">The reference to the starting position of the array.</param>
        /// <param name="size">The size of the array.</param>
        /// <param name="received">The actual numbers of numbers received.</param>
        /// <returns>The return code for the connect (the int SuccessReturnCode is returned in
        /// case of success).</returns>
        [DllImport("libqrng.qrng")]
        private static extern int qrng_get_double_array(ref double array, int size, ref int received);

        /// <summary>
        /// Disconnects from the webservice (if already connected).
        /// This is a call to the dll providing the service.
        /// </summary>
        /// <returns>The return code for the connect (the int SuccessReturnCode is returned in
        /// case of success).</returns>
        [DllImport("libqrng.qrng")]
        private static extern int qrng_disconnect();

        /// <summary>
        /// Connects to the webservice using the given username and password.
        /// </summary>
        /// <param name="username">The username for the webservice.</param>
        /// <param name="password">The password for the websevice.</param>
        /// <returns>The return code for the connect (the int SuccessReturnCode is returned in
        /// case of success).</returns>
        public static int Connect(string username, string password)
        {
            return qrng_connect(username, password);
        }

        /// <summary>
        /// Fills an array of double with random numbers received from the webservice.
        /// </summary>
        /// <param name="array">The reference to the starting position of the array.</param>
        /// <param name="size">The size of the array.</param>
        /// <param name="received">The actual numbers of numbers received.</param>
        /// <returns>The return code for the connect (the int SuccessReturnCode is returned in
        /// case of success).</returns>
        public static int GetDoubleArray(ref double array, int size, ref int received)
        {
            return qrng_get_double_array(ref array, size, ref received);
        }

        /// <summary>
        /// Disconnects from the webservice (if already connected).
        /// </summary>
        /// <returns>The return code for the connect (the int SuccessReturnCode is returned in
        /// case of success).</returns>
        public static int Disconnect()
        {
            return qrng_disconnect();
        }
    }
}
