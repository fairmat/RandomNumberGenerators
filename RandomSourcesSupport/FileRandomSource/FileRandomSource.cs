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
using System.Globalization;
using System.IO;
using System.Linq;
using DVPLI;
using Mono.Addins;

namespace RandomSourcesSupport.FileRandomSource
{
    /// <summary>
    /// Implements the module responsible of loading the random numbers from a certain file
    /// (binary or text file).
    /// </summary>
    [Extension("/RandomSourcesSupport/RandomSource")]
    public class FileRandomSource : IRandomSource, IDescription
    {
        #region Fields
        /// <summary>
        /// The path of the file that acts as a random source.
        /// </summary>
        private string filePath;

        /// <summary>
        /// The type of the file that acts as a random source.
        /// </summary>
        private FileType fileType;

        /// <summary>
        /// The size (expressed as number of double values) of each block
        /// that has to be read from the file.
        /// </summary>
        private const int blockSize = 100000;

        /// <summary>
        /// The actual size (expressed as number of double values) of the current block.
        /// </summary>
        private int blockEffectiveSize;

        /// <summary>
        /// The number of the block that has to be read.
        /// </summary>
        private int blockNumber;

        /// <summary>
        /// The buffer used to store the block of randomly generated numbers.
        /// </summary>
        private double[] block;

        /// <summary>
        /// The current index in the block.
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// True if the loading can be repeated in case the previous loading block has failed.
        /// </summary>
        private bool canReload = true;

        /// <summary>
        /// True if the first load has yet to be performed, false otherwise.
        /// </summary>
        private bool firstLoad = true;
        #endregion // Fields

        #region Constructors
        /// <summary>
        /// Initializes the random source.
        /// </summary>
        public FileRandomSource()
        {
            FileRandomSourceSettings settings = UserSettings.GetSettings(typeof(FileRandomSourceSettings)) as FileRandomSourceSettings;
            this.filePath = settings.FilePath;
            this.fileType = settings.FileType;

            // If the file is binary allocate the block array in advance
            if (this.fileType == FileType.Binary)
                this.block = new double[blockSize];
        }
        #endregion // Constructors

        #region IRandomSource implementation
        /// <summary>
        /// Initializes the source in order to obtain a random sequence.
        /// </summary>
        public void InitializeNonRepeatable()
        {
            // Calculate the maximum sequence Id that can be used
            int maxSequenceId;
            if (this.fileType == FileType.Binary)
            {
                // Find out how many numbers are stored in the file
                long numbersCount = 0;
                if (File.Exists(this.filePath))
                {
                    using (FileStream stream = new FileStream(this.filePath, FileMode.Open))
                    {
                        numbersCount = stream.Length;
                    }
                }

                numbersCount /= sizeof(double);
                maxSequenceId = (int)(numbersCount % int.MaxValue);
            }
            else
                maxSequenceId = File.ReadAllLines(this.filePath).Length;

            // Return a random sequence id between 0 and its max value
            Random r = new Random();
            InitializeRepeatable(r.Next(maxSequenceId));
        }

        /// <summary>
        /// Initializes the source in order to repeat an existing sequence.
        /// <para>If the sequence doesn't exist the default sequence is used instead.</para>
        /// </summary>
        /// <param name="sequenceId">The Id of the sequence to repeat.</param>
        public void InitializeRepeatable(int sequenceId)
        {
            // Use the sequenceId as the counter of double that was generated before
            this.currentIndex = sequenceId % blockSize;
            LoadBlock(this.blockNumber = sequenceId / blockSize);
        }

        /// <summary>
        /// Restore the state of the source based on the given object.
        /// </summary>
        /// <param name="restorePoint">The restore point.</param>
        public void LoadState(IRandomGeneratorRestorePoint restorePoint)
        {
            FileRandomSourceRestorePoint rp = restorePoint as FileRandomSourceRestorePoint;
            if (rp == null)
                return;

            InitializeRepeatable(rp.SequenceId);
        }

        /// <summary>
        /// Gets the state of the generator.
        /// </summary>
        /// <returns>An object representing the status of the generator.</returns>
        public IRandomGeneratorRestorePoint GetState()
        {
            return new FileRandomSourceRestorePoint((this.blockNumber * blockSize) + this.currentIndex);
        }

        /// <summary>
        /// Gets the next random number.
        /// </summary>
        /// <returns>A random uniform number.</returns>
        public double Next()
        {
            if (this.currentIndex < this.blockEffectiveSize)
                return this.block[this.currentIndex++];
            else
            {
                LoadBlock();
                return this.block[this.currentIndex = 0];
            }
        }
        #endregion // IRandomSource implementation

        #region Block load
        /// <summary>
        /// Loads a block of randomly generated numbers from the file used as a random source.
        /// </summary>
        private void LoadBlock()
        {
            LoadBlock(this.blockNumber++);

            // If the actual number of doubles read is 0 try to reload the first block
            if (this.blockEffectiveSize == 0)
            {
                if (this.canReload)
                {
                    // If the block can be reloaded try to load the first block
                    this.canReload = false;
                    LoadBlock(this.blockNumber = 0);
                }
                else
                {
                    // If the block can't be reloaded (in case of two loading errors in a row)
                    // throw an exception
                    throw new Exception("Can't read doubles from " + this.filePath);
                }
            }
            else
                this.canReload = true;
        }

        /// <summary>
        /// Loads the specified block of randomly generated numbers
        /// from the file used as a random source.
        /// </summary>
        /// <param name="blockNumber">The index of the block to load.</param>
        private void LoadBlock(int blockNumber)
        {
            // If this is the first load update the file type (in case the user edited it)
            if (this.firstLoad)
            {
                this.firstLoad = false;
                FileRandomSourceSettings settings = UserSettings.GetSettings(typeof(FileRandomSourceSettings)) as FileRandomSourceSettings;
                this.fileType = settings.FileType;
            }

            // Load the block based on the file type
            if (this.fileType == FileType.Binary)
                LoadBlockBinary(blockNumber);
            else
                LoadBlockText(blockNumber);
        }

        /// <summary>
        /// Loads the specified block of randomly generated numbers from the binary file used as a
        /// random source.
        /// </summary>
        /// <param name="blockNumber">The index of the block to load.</param>
        private void LoadBlockBinary(int blockNumber)
        {
            if (!File.Exists(this.filePath))
                throw new Exception(this.filePath + " does not exist.");

            using (FileStream stream = new FileStream(this.filePath, FileMode.Open))
            {
                long filePosition = stream.Seek(blockSize * blockNumber * sizeof(double), SeekOrigin.Begin);

                if (filePosition >= stream.Length)
                    this.blockEffectiveSize = 0;
                else
                {
                    if (filePosition + blockSize < stream.Length)
                        this.blockEffectiveSize = blockSize;
                    else
                        this.blockEffectiveSize = (int)(stream.Length - filePosition);

                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        for (int i = 0; i < this.blockEffectiveSize; i++)
                        {
                            this.block[i] = reader.ReadDouble();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the specified block of randomly generated numbers from the text file used
        /// as a random source.
        /// </summary>
        /// <param name="blockNumber">The index of the block to load.</param>
        private void LoadBlockText(int blockNumber)
        {
            if (!File.Exists(this.filePath))
                throw new Exception(this.filePath + " does not exist.");

            // More than one number per line separated by a space, file size relatively small
            // because of ReadAllLines
            this.block = File.ReadAllLines(this.filePath)
                        .SelectMany(s => s.Split(' '))
                        .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                        .Select((value, index) => new { Value = value, Index = index })
                        .Where(x => x.Index >= blockNumber * blockSize &&
                               x.Index < (blockNumber + 1) * blockSize)
                        .Select(x => x.Value)
                        .ToArray();

            this.blockEffectiveSize = this.block.Length;
        }
        #endregion // Block load

        #region IDescription implementation
        /// <summary>
        /// Gets the description for the random source.
        /// </summary>
        public string Description
        {
            get
            {
                return "Random numbers from file";
            }
        }
        #endregion // IDescription implementation
    }
}
