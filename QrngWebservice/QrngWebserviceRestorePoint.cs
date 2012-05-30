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

namespace QrngWebservice
{
    /// <summary>
    /// Represents a restore point for the quantum random numbers generator
    /// using the qrng.physik.hu-berlin.de webservice.
    /// </summary>
    public class QrngWebserviceRestorePoint : IRandomGeneratorRestorePoint
    {
        #region Properties
        /// <summary>
        /// Gets the sequence Id.
        /// </summary>
        public int SequenceId { get; private set; }

        /// <summary>
        /// Gets the position in the file relative to the sequence Id.
        /// </summary>
        public long Position { get; private set; }
        #endregion //Properties

        #region Constructors
        /// <summary>
        /// Initialized the restore point.
        /// </summary>
        /// <param name="sequenceId">The sequence Id.</param>
        /// <param name="position">The position in the file.</param>
        public QrngWebserviceRestorePoint(int sequenceId, long position)
        {
            this.SequenceId = sequenceId;
            this.Position = position;
        }
        #endregion // Constructors
    }
}
