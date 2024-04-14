using System;
using System.IO;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.City
{
    public class CityDataReader : IDataReader
    {
        readonly string filePath = Application.dataPath + "/StreamingAssets/SeineSaintDenis/Shape/limits.dat";

        protected char[] buffer;
        protected int bufferReadIndex;
        protected int bufferSize;

        protected char[] convertionBuffer;
        protected int convertionBufferReadIndex;
        protected int maxConvertionSizeOfData;

        protected bool endOfStream;

        public bool EndOfStream
        {
            get { return endOfStream; }
        }

        protected StreamReader streamReader;
        protected bool firstRead;

        bool isAtData = false;

        public CityDataReader() : this(50, 100)
        {
        }


        public CityDataReader(int _bufferSize, int _maxConvertionSizeOfData)
        {
            firstRead = true;

            bufferSize = _bufferSize;
            bufferReadIndex = _bufferSize; // to read buffer at first read
            buffer = new char[bufferSize];

            maxConvertionSizeOfData = _maxConvertionSizeOfData;
            convertionBuffer = new char[_maxConvertionSizeOfData];
            convertionBufferReadIndex = 0;
        }

        protected bool IsInited()
        {
            if (streamReader == null || firstRead)
            {
                throw new System.InvalidOperationException("DataReader started operation without initing first.");
            }

            return streamReader == null;
        }

        public void Init()
        {
            streamReader = new StreamReader(filePath);
            firstRead = true;

            //first read filling up buffer
            if (bufferReadIndex == bufferSize)
            {
                streamReader.ReadBlock(buffer, 0, bufferSize);
                bufferReadIndex = 0;
            }
        }

        public void Clean()
        {
            streamReader.Close();
            bufferReadIndex = 0;
            convertionBufferReadIndex = 0;

            Array.Clear(convertionBuffer, 0, maxConvertionSizeOfData);
            Array.Clear(buffer, 0, bufferSize);
        }


        public IData GetData()
        {
            IsInited();

            if (!isAtData)
            {
                throw new System.FormatException(
                    "CityDataReader tried to getData() when not being place in front of Data");
            }

            if (EndOfStream)
            {
                throw new System.OverflowException("CityDataReader went to end of file while try to getData()");
            }

            float tmpX, tmpY;

            int read = ReadUntilDelimiterChar(ref convertionBuffer, ',');
            Array.Clear(convertionBuffer, read, maxConvertionSizeOfData - read);

            try
            {
                tmpX = float.Parse(new string(convertionBuffer), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new FormatException("Couldn't parse : \"" + new string(convertionBuffer) + "\"");
            }

            read = ReadUntilDelimiterChar(ref convertionBuffer, ']');
            Array.Clear(convertionBuffer, read, maxConvertionSizeOfData - read);

            try
            {
                tmpY = float.Parse(new string(convertionBuffer), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new FormatException("Couldn't parse : \"" + new string(convertionBuffer) + "\"");
            }

            isAtData = false;
            return new CityData(tmpX, tmpY);
        }

        public void GoToNextData()
        {
            char[] tmpBuff = new char[bufferSize];
            if (firstRead)
            {
                bufferReadIndex = 2;
                firstRead = false;
            }
            else
            {
                ReadUntilDelimiterChar(ref tmpBuff, '[');
            }

            isAtData = true && !EndOfStream;
        }


        /*
         * @out readBytes
         * @in buff buffer that will be read into
         * @in delimiter dellimiter to stop reading
         */
        protected int ReadUntilDelimiterChar(ref char[] buff, char delimiter, bool includeDelimiter = false)
        {
            int totalRead = 0;

            while (!endOfStream && buffer[bufferReadIndex] != delimiter)
            {
                if (streamReader.EndOfStream)
                {
                    endOfStream = true;
                    //return totalRead;
                }

                //take a char from the buffer put it inside input buffer
                try
                {
                    buff[totalRead] = buffer[bufferReadIndex];
                }
                catch (System.Exception)
                {
                    throw new System.IndexOutOfRangeException(
                        "The buffer is to small for the read data.\nFirst bytes of data : " + new string(buff));
                }

                bufferReadIndex++;
                totalRead++;

                //if buffer is empty , read another buffer
                if (bufferReadIndex == bufferSize)
                {
                    streamReader.ReadBlock(buffer, 0, bufferSize);
                    bufferReadIndex = 0;
                }
            }

            if (!includeDelimiter)
            {
                bufferReadIndex++;

                //if buffer is empty , read another buffer
                if (bufferReadIndex == bufferSize)
                {
                    streamReader.ReadBlock(buffer, 0, bufferSize);
                    bufferReadIndex = 0;
                }
            }

            return totalRead;
        }
    }
}