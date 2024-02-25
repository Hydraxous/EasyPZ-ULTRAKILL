using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace EasyPZ.Ghosts
{
    public static class GhostFileReaderFactory
    {

        private static readonly GhostFileReaderV0 readerV0 = new GhostFileReaderV0();
        private static readonly GhostFileReaderV1 readerV1 = new GhostFileReaderV1();
        private static readonly GhostFileReaderV2 readerV2 = new GhostFileReaderV2();

        public static IGhostFileReader GetReader(int fileType)
        {
            switch(fileType)
            {
                case 0:
                    return readerV0;
                case 1:
                    return readerV1;
                case 2:
                    return readerV2;
                default:
                    throw new Exception($"Unknown ghost file type {fileType}. Not supported in this version.");
            }
        }
    }

    public interface IGhostFileReader
    {
        public GhostRecording Read(BinaryReader br);
        public GhostRecordingMetadata ReadMetadata(BinaryReader br);
        public GhostRecordingFrame ReadFrame(BinaryReader br);
    }

}
