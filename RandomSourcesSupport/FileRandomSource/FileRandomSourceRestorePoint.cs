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

namespace RandomSourcesSupport.FileRandomSource
{
    /// <summary>
    /// Represents a restore point for the random numbers generator using a
    /// file for getting the numbers.
    /// </summary>
    public class FileRandomSourceRestorePoint : IRandomGeneratorRestorePoint
    {
        #region Properties
        /// <summary>
        /// Gets the sequence Id.
        /// </summary>
        public int SequenceId { get; private set; }
        #endregion //Properties

        #region Constructors
        /// <summary>
        /// Initialized the restore point.
        /// </summary>
        /// <param name="sequenceId">The sequence Id.</param>
        public FileRandomSourceRestorePoint(int sequenceId)
        {
            this.SequenceId = sequenceId;
        }
        #endregion // Constructors
    }
}
