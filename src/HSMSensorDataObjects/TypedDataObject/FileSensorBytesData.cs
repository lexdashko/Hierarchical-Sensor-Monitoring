﻿using System.Runtime.Serialization;

namespace HSMSensorDataObjects.TypedDataObject
{
    [DataContract]
    public class FileSensorBytesData
    {
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string Extension { get; set; }
        [DataMember]
        public byte[] FileContent { get; set; }
        [DataMember]
        public string FileName { get; set; }
    }
}
