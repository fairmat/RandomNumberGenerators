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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using DVPLI;
using Mono.Addins;
using RandomSourcesSupport;

namespace QrngWebservice
{
    /// <summary>
    /// Implements the module responsible of downloading the new data from the
    /// webservice (qrng.physik.hu-berlin.de) when is needed and handling the
    /// persistence of the data on the hard drive.
    /// </summary>
    [Extension("/RandomSourcesSupport/RandomSource")]
    public class QrngWebserviceRandomSource : IRandomSource, IDescription
    {
        #region Fields
        /// <summary>
        /// The singleton instance for this class.
        /// </summary>
        private static QrngWebserviceRandomSource singletonInstance;

        /// <summary>
        /// The default path where the randomly generated sequences are stored.
        /// </summary>
        private string defaultPath = DVPLI.UserSettingFolder.Location;

        /// <summary>
        /// The number of buffers used for storing part of the generated numbers.
        /// </summary>
        private const int buffersCount = 2;

        /// <summary>
        /// The index of the current buffer.
        /// </summary>
        private int currentBufferIndex;

        /// <summary>
        /// The list of buffers.
        /// </summary>
        private List<double[]> buffers = new List<double[]>();

        /// <summary>
        /// The current block loaded in memory.
        /// </summary>
        private int currentBlock = -1;

        /// <summary>
        /// The number of randomly generated numbers to download from the webservice with each call.
        /// </summary>
        private const int numbersPerBlock = 100000;

        /// <summary>
        /// The maximum numbers of random values that can be saved in a single file.
        /// </summary>
        private const int maximumNumbersPerFile = 64000000;

        /// <summary>
        /// The current index in the random values array.
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// The current sequence Id to use for storing/loading the data.
        /// </summary>
        private int sequenceId;

        /// <summary>
        /// The maximum value that the sequence Id can reach (from the starting point 0).
        /// </summary>
        private const int maximumSequenceId = 9;

        /// <summary>
        /// The thread used for loading the data in background.
        /// </summary>
        private Thread loadingThread;

        /// <summary>
        /// The starting sequence Id.
        /// </summary>
        private int startingSequenceId;

        /// <summary>
        /// The position relative to the starting sequence Id.
        /// </summary>
        private long position;
        #endregion // Fields

        #region Properties
        /// <summary>
        /// Gets the instance of this class.
        /// </summary>
        public static QrngWebserviceRandomSource Instance
        {
            get
            {
                if (singletonInstance == null)
                    singletonInstance = new QrngWebserviceRandomSource();

                return singletonInstance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the plugin is connected to the webservice.
        /// </summary>
        public bool Connected { get; private set; }
        #endregion // Properties

        #region Constructors
        /// <summary>
        ///  Initializes the module.
        /// </summary>
        public QrngWebserviceRandomSource()
        {
            // Verify the existence of the path for storing the sequence
            this.defaultPath = Path.Combine(DVPLI.UserSettingFolder.Location, "RngSequences");
            if (!Directory.Exists(this.defaultPath))
                Directory.CreateDirectory(this.defaultPath);

            for (int i = 0; i < buffersCount; i++)
            {
                this.buffers.Add(new double[numbersPerBlock]);
            }

            this.currentIndex = numbersPerBlock;
        }
        #endregion // Constructors

        #region Destructor
        /// <summary>
        /// Disconnect if the object is destroyed.
        /// </summary>
        ~QrngWebserviceRandomSource()
        {
            Disconnect();
        }
        #endregion // Destructor

        #region IRandomSource implementation
        /// <summary>
        /// Initializes the source in order to obtain a random sequence.
        /// </summary>
        public void InitializeNonRepeatable()
        {
            // Get a random sequence Id (capped to the maximum number it can reach)
            Random r = new Random();
            int rndId = r.Next(maximumSequenceId + 1);

            // Find out the effective numbers of values in the file
            string filePath = Path.Combine(this.defaultPath, "RngSequence" + rndId.ToString());
            long numbersCount = 0;
            if (File.Exists(filePath))
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    numbersCount = stream.Length;
                }
            }

            numbersCount /= sizeof(double);

            if (numbersCount == 0)
            {
                // If there are no numbers in the file initialize the sequence from the start
                InitializeRepeatable(rndId);
            }
            else
            {
                // Otherwise initialize it from a random position (valid for the given file)
                double d = r.NextDouble();
                long rndPosition = (long)(d * numbersCount);
                QrngWebserviceRestorePoint rp = new QrngWebserviceRestorePoint(rndId, rndPosition);
                Connect();
                LoadState(rp);
            }
        }

        /// <summary>
        /// Initializes the source in order to repeat an existing sequence.
        /// <para>If the sequence doesn't exist it's created.</para>
        /// </summary>
        /// <param name="sequenceId">The Id of the sequence to repeat.</param>
        public void InitializeRepeatable(int sequenceId)
        {
            // If the data is still being loaded on the background thread wait for it to end
            // and set it to null
            if (this.loadingThread != null)
            {
                this.loadingThread.Join();
                this.loadingThread = null;
            }

            // Initialize the status of the random source
            this.sequenceId = sequenceId;
            this.startingSequenceId = sequenceId;
            this.position = 0;

            // Connect to the webservice
            Connect();

            // Download the first buffer synchronously so the first block of data is already
            // available
            this.currentBlock = -1;
            DownloadBlock(0);
            this.currentIndex = 0;

            // Download the second buffer asynchronously
            DownloadBlockAsynchronous(1);
        }

        /// <summary>
        /// Restore the state of the source based on the given object.
        /// </summary>
        /// <param name="restorePoint">The restore point.</param>
        public void LoadState(IRandomGeneratorRestorePoint restorePoint)
        {
            QrngWebserviceRestorePoint rp = restorePoint as QrngWebserviceRestorePoint;
            if (rp == null)
                return;

            // If the data is still being loaded on the background thread wait for it to end
            // and set it to null
            if (this.loadingThread != null)
            {
                this.loadingThread.Join();
                this.loadingThread = null;
            }

            // Set the starting Id and position
            this.startingSequenceId = rp.SequenceId;
            this.position = rp.Position;

            // Calculate the state parameters
            int unadjustedBlockNo = (int)(rp.Position / numbersPerBlock);
            int sequenceOffset = this.currentBlock / (maximumNumbersPerFile / numbersPerBlock);
            this.sequenceId = this.startingSequenceId + sequenceOffset;
            this.currentBlock = unadjustedBlockNo % (maximumNumbersPerFile / numbersPerBlock);
            this.currentIndex = (int)(this.position - (unadjustedBlockNo * numbersPerBlock));

            // Load the buffers (the first synchronously, the second asynchronously)
            LoadRngNumbers(this.currentBlock, 0);
            this.currentBufferIndex = 0;
            if (!double.IsNaN(this.buffers[0][0]))
            {
                DownloadBlockAsynchronous(1);
            }
        }

        /// <summary>
        /// Gets the state of the generator.
        /// </summary>
        /// <returns>An object representing the status of the generator.</returns>
        public IRandomGeneratorRestorePoint GetState()
        {
            return new QrngWebserviceRestorePoint(this.startingSequenceId, this.position);
        }

        /// <summary>
        /// Gets the next random uniform number.
        /// </summary>
        /// <returns>A random uniform number.</returns>
        public double Next()
        {
            if (this.currentIndex < this.buffers[this.currentBufferIndex].Length)
            {
                // The buffer has not reach its limit yet
                this.position++;
                return this.buffers[this.currentBufferIndex][this.currentIndex++];
            }
            else
            {
                // All the elements of the current buffer has been accessed
                // Use Join just to be sure the background loading ended
                if (this.loadingThread != null)
                    this.loadingThread.Join();

                // Download the new block asynchronously in the current buffer, update the
                // indexes and return the next number from the other buffer
                DownloadBlockAsynchronous(this.currentBufferIndex);
                this.currentBufferIndex = (this.currentBufferIndex + 1) % buffersCount;
                this.currentIndex = 0;
                return Next();
            }
        }
        #endregion // IRandomSource implementation

        #region Connection/Disconnection
        /// <summary>
        /// Connect to the Qrng webservice using the credentials stored in the user settings.
        /// </summary>
        public void Connect()
        {
            // If the plugin is already connected a Disconnect has to be performed first in
            // order to reconnect again so nothing is done
            if (Connected)
                return;

            QrngWebserviceSettings settings = UserSettings.GetSettings(typeof(QrngWebserviceSettings)) as QrngWebserviceSettings;
            int errorNo = LibQrng.Connect(settings.Username, settings.Password.Value);
            Connected = errorNo == LibQrng.SuccessReturnCode;
        }

        /// <summary>
        /// Disconnect from the Qrng webservice.
        /// </summary>
        public void Disconnect()
        {
            // If the data is still being loaded on the background thread wait for it to end.
            if (this.loadingThread != null)
                this.loadingThread.Join();

            if (Connected)
                LibQrng.Disconnect();
            Connected = false;
        }
        #endregion // Connection/Disconnection

        #region Random data download
        /// <summary>
        /// Gets the randomly generated numbers from the webservice and stores it in the array.
        /// </summary>
        /// <param name="array">The array used to store the values.</param>
        /// <returns>true if the value has been generated correctly, false otherwise.</returns>
        private bool GetRngValues(double[] array)
        {
            // If the plugin is not connected to the webservice abort
            if (!Connected)
                return false;

            int doublesGenerated = 0;
            int errorNo = LibQrng.GetDoubleArray(ref array[0], array.Length, ref doublesGenerated);
            return errorNo == LibQrng.SuccessReturnCode;
        }

        /// <summary>
        /// Invalidates the array of double by setting all its element to NaN.
        /// </summary>
        /// <param name="array">The array to invalidate.</param>
        private void InvalidateArray(double[] array)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = double.NaN;
        }

        /// <summary>
        /// Downloads a new block of random data asynchronously.
        /// </summary>
        /// <param name="bufferIndex">The index of the buffer to use.</param>
        private void DownloadBlockAsynchronous(int bufferIndex)
        {
            this.loadingThread = new Thread(new ThreadStart(() => DownloadBlock(bufferIndex)));
            this.loadingThread.IsBackground = true;
            this.loadingThread.Start();
        }

        /// <summary>
        /// Downloads a new block of random data.
        /// </summary>
        /// <param name="bufferIndex">The index of the buffer to use.</param>
        private void DownloadBlock(int bufferIndex)
        {
            // Current block can be accessed by other threads but in this implementation only one
            // thread that performs the loading in background can be active at any time. There is
            // no need to lock the variable or handle concurrency.
            ++this.currentBlock;

            // Try to load the current block
            LoadRngNumbers(this.currentBlock, bufferIndex);

            // If the loading fails the buffer is invalidated so if the buffer has NaN as a first
            // element get the numbers using the webservice.
            if (double.IsNaN(this.buffers[bufferIndex][0]))
            {
                if (GetRngValues(this.buffers[bufferIndex]))
                    SaveRngNumbers(bufferIndex);
                else
                    InvalidateArray(this.buffers[bufferIndex]);
            }
        }
        #endregion // Random data download

        #region Block Save/Load
        /// <summary>
        /// Saves the buffer with the given index in the default location,
        /// appending it if it already exists.
        /// </summary>
        /// <param name="bufferIndex">The index of the buffer to use.</param>
        private void SaveRngNumbers(int bufferIndex)
        {
            SaveRngNumbers(Path.Combine(this.defaultPath, "RngSequence" + this.sequenceId.ToString()), bufferIndex);
        }

        /// <summary>
        /// Saves the buffer with the given index at the specified
        /// location, appending it if it already exists.
        /// </summary>
        /// <param name="filePath">The path of the file to update/create.</param>
        /// <param name="bufferIndex">The index of the buffer to use.</param>
        private void SaveRngNumbers(string filePath, int bufferIndex)
        {
            if (!File.Exists(filePath))
                using (FileStream stream = new FileStream(filePath, FileMode.Create)) { }

            using (FileStream stream = new FileStream(filePath, FileMode.Append))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < this.buffers[bufferIndex].Length; i++)
                    {
                        writer.Write(this.buffers[bufferIndex][i]);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the given block number from the default file
        /// into the buffer with the specified index.
        /// </summary>
        /// <param name="blockNo">The number of the block to load (zero based index).</param>
        /// <param name="bufferIndex">The index of the buffer to use.</param>
        private void LoadRngNumbers(int blockNo, int bufferIndex)
        {
            if ((blockNo + 1) * numbersPerBlock > maximumNumbersPerFile)
            {
                this.sequenceId = (this.sequenceId + 1) % (maximumSequenceId + 1);
                this.currentBlock = blockNo = 0;
            }

            LoadRngNumbers(Path.Combine(this.defaultPath, "RngSequence" + this.sequenceId.ToString()), blockNo, bufferIndex);
        }

        /// <summary>
        /// Loads the given block number from the given file path into
        /// the buffer with the specified index.
        /// </summary>
        /// <param name="filePath">The file containing the random data.</param>
        /// <param name="blockNo">The number of the block to load (zero based index).</param>
        /// <param name="bufferIndex">The index of the buffer to use.</param>
        private void LoadRngNumbers(string filePath, int blockNo, int bufferIndex)
        {
            if (!File.Exists(filePath))
                using (FileStream stream = new FileStream(filePath, FileMode.Create)) { }

            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                long filePosition = stream.Seek(numbersPerBlock * blockNo * sizeof(double), SeekOrigin.Begin);

                if (filePosition >= stream.Length)
                    InvalidateArray(this.buffers[bufferIndex]);
                else
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        for (int i = 0; i < this.buffers[bufferIndex].Length; i++)
                        {
                            this.buffers[bufferIndex][i] = reader.ReadDouble();
                        }
                    }
                }
            }
        }
        #endregion // Block Save/Load

        #region IDescription implementation
        /// <summary>
        /// Gets the description for the random source.
        /// </summary>
        public string Description
        {
            get
            {
                return "Qrng.physik.hu-berlin webservice";
            }
        }
        #endregion // IDescription implementation
    }
}
