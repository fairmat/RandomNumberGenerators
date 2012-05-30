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

using DVPLI;
using Mono.Addins;

namespace RandomSourcesSupport
{
    /// <summary>
    /// Extension for creating an object capable of producing a sequence of random numbers.
    /// </summary>
    [TypeExtensionPoint("/RandomSourcesSupport/RandomSource")]
    public interface IRandomSource
    {
        /// <summary>
        /// Initializes the source in order to obtain a random sequence.
        /// </summary>
        void InitializeNonRepeatable();

        /// <summary>
        /// Initializes the source in order to repeat an existing sequence.
        /// <para>If the sequence doesn't exist it's created.</para>
        /// </summary>
        /// <param name="sequenceId">The Id of the sequence to repeat.</param>
        void InitializeRepeatable(int sequenceId);

        /// <summary>
        /// Restore the state of the source based on the given object.
        /// </summary>
        /// <param name="restorePoint">The restore point.</param>
        void LoadState(IRandomGeneratorRestorePoint restorePoint);

        /// <summary>
        /// Gets the state of the generator.
        /// </summary>
        /// <returns>An object representing the status of the generator.</returns>
        IRandomGeneratorRestorePoint GetState();

        /// <summary>
        /// Gets the next random uniform number.
        /// </summary>
        /// <returns>A random uniform number.</returns>
        double Next();
    }
}
