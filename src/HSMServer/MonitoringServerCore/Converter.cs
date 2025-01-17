﻿using System;
using System.Text.Json;
using HSMCommon.Model;
using HSMCommon.Model.SensorsData;
using HSMSensorDataObjects;
using HSMSensorDataObjects.FullDataObject;
using HSMSensorDataObjects.TypedDataObject;
using HSMServer.DataLayer.Model;
using HSMServer.Model;
using Microsoft.Extensions.Logging;

namespace HSMServer.MonitoringServerCore
{
    internal class Converter : IConverter
    {
        private readonly ILogger<Converter> _logger;
        private const double SIZE_DENOMINATOR = 1024.0;

        //public SignedCertificateMessage Convert(X509Certificate2 signedCertificate,
        //    X509Certificate2 caCertificate)
        //{
        //    SignedCertificateMessage message = new SignedCertificateMessage();
        //    message.CaCertificateBytes = ByteString.CopyFrom(caCertificate.Export(X509ContentType.Cert));
        //    message.SignedCertificateBytes = ByteString.CopyFrom(signedCertificate.Export(X509ContentType.Pfx));
        //    return message;
        //}

        public Converter(ILogger<Converter> logger)
        {
            _logger = logger;
        }

        #region Deserialize

        public BoolSensorValue GetBoolSensorValue(string json)
        {
            return JsonSerializer.Deserialize<BoolSensorValue>(json);
        }

        public IntSensorValue GetIntSensorValue(string json)
        {
            return JsonSerializer.Deserialize<IntSensorValue>(json);
        }

        public DoubleSensorValue GetDoubleSensorValue(string json)
        {
            return JsonSerializer.Deserialize<DoubleSensorValue>(json);
        }

        public StringSensorValue GetStringSensorValue(string json)
        {
            return JsonSerializer.Deserialize<StringSensorValue>(json);
        }
        public IntBarSensorValue GetIntBarSensorValue(string json)
        {
            return JsonSerializer.Deserialize<IntBarSensorValue>(json);
        }

        public DoubleBarSensorValue GetDoubleBarSensorValue(string json)
        {
            return JsonSerializer.Deserialize<DoubleBarSensorValue>(json);
        }

        public FileSensorValue GetFileSensorValue(string json)
        {
            return JsonSerializer.Deserialize<FileSensorValue>(json);
        }

        #endregion

        #region Convert to history items

        public SensorHistoryData Convert(ExtendedBarSensorData data)
        {
            switch (data.ValueType)
            {
                case SensorType.DoubleBarSensor:
                    return Convert(data.Value as DoubleBarSensorValue, data.TimeCollected);
                case SensorType.IntegerBarSensor:
                    return Convert(data.Value as IntBarSensorValue, data.TimeCollected);
                default:
                    return null;
            }
        }

        //private SensorHistoryMessage Convert(IntBarSensorValue value, DateTime timeCollected)
        //{
        //    SensorHistoryMessage result = new SensorHistoryMessage();
        //    try
        //    {
        //        result.TypedData = JsonSerializer.Serialize(ToTypedData(value));
        //        result.Time = Timestamp.FromDateTime(value.Time.ToUniversalTime());
        //        result.Type = SensorObjectType.ObjectTypeBarIntSensor;
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    return result;
        //}
        private SensorHistoryData Convert(IntBarSensorValue value, DateTime timeCollected)
        {
            SensorHistoryData result = new SensorHistoryData();
            try
            {
                result.TypedData = JsonSerializer.Serialize(ToTypedData(value));
                result.Time = value.Time;
                result.SensorType = SensorType.IntegerBarSensor;
            }
            catch (Exception e)
            { }

            return result;
        }
        //private SensorHistoryMessage Convert(DoubleBarSensorValue value, DateTime timeCollected)
        //{
        //    SensorHistoryMessage result = new SensorHistoryMessage();
        //    try
        //    {
        //        result.TypedData = JsonSerializer.Serialize(ToTypedData(value));
        //        result.Time = Timestamp.FromDateTime(value.Time.ToUniversalTime());
        //        result.Type = SensorObjectType.ObjectTypeBarDoubleSensor;
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    return result;
        //}
        private SensorHistoryData Convert(DoubleBarSensorValue value, DateTime timeCollected)
        {
            SensorHistoryData result = new SensorHistoryData();
            try
            {
                result.TypedData = JsonSerializer.Serialize(ToTypedData(value));
                result.Time = value.Time;
                result.SensorType = SensorType.DoubleBarSensor;
            }
            catch (Exception e)
            { }

            return result;
        }
        //public SensorHistoryMessage Convert(SensorDataObject dataObject)
        //{
        //    SensorHistoryMessage result = new SensorHistoryMessage();
        //    try
        //    {
        //        result.TypedData = dataObject.TypedData;
        //        result.Time = Timestamp.FromDateTime(dataObject.Time.ToUniversalTime());
        //        result.Type = Convert(dataObject.DataType);
        //    }
        //    catch (Exception e)
        //    {
                
        //    }
        //    return result;
        //}
        public SensorHistoryData Convert(SensorDataObject dataObject)
        {
            SensorHistoryData historyData = new SensorHistoryData();
            try
            {
                historyData.TypedData = dataObject.TypedData;
                historyData.Time = dataObject.Time;
                historyData.SensorType = dataObject.DataType;
            }
            catch (Exception e)
            { }

            return historyData;
        }

        #endregion
        #region Convert to database objects

        private void FillCommonFields(SensorValueBase value, DateTime timeCollected, out SensorDataObject dataObject)
        {
            dataObject = new SensorDataObject();
            dataObject.Path = value.Path;
            dataObject.Time = value.Time.ToUniversalTime();
            dataObject.TimeCollected = timeCollected.ToUniversalTime();
            dataObject.Timestamp = GetTimestamp(value.Time);
        }

        //private SensorType Convert(SensorObjectType type)
        //{
        //    switch (type)
        //    {
        //        case SensorObjectType.ObjectTypeBoolSensor:
        //            return SensorType.BooleanSensor;
        //        case SensorObjectType.ObjectTypeDoubleSensor:
        //            return SensorType.DoubleSensor;
        //        case SensorObjectType.ObjectTypeIntSensor:
        //            return SensorType.IntSensor;
        //        case SensorObjectType.ObjectTypeStringSensor:
        //            return SensorType.StringSensor;
        //        case SensorObjectType.ObjectTypeBarDoubleSensor:
        //            return SensorType.DoubleBarSensor;
        //        case SensorObjectType.ObjectTypeBarIntSensor:
        //            return SensorType.IntegerBarSensor;
        //        case SensorObjectType.ObjectTypeFileSensor:
        //            return SensorType.FileSensor;
        //        default:
        //            throw new InvalidEnumArgumentException($"Invalid SensorDataType: {type}");
        //    }
        //}
        //public SensorDataObject ConvertToDatabase(SensorUpdateMessage update, DateTime originalTime)
        //{
        //    SensorDataObject result = new SensorDataObject();
        //    result.Path = update.Path;
        //    result.Time = originalTime;
        //    result.TypedData = update.DataObject.ToString(Encoding.UTF8);
        //    result.TimeCollected = update.Time.ToDateTime();
        //    result.Timestamp = GetTimestamp(result.TimeCollected);
        //    result.DataType = Convert(update.ObjectType);
        //    return result;
        //}

        public SensorDataObject ConvertToDatabase(BoolSensorValue sensorValue, DateTime timeCollected)
        {
            SensorDataObject result;
            FillCommonFields(sensorValue, timeCollected, out result);
            result.DataType = SensorType.BooleanSensor;
            result.Status = sensorValue.Status;

            BoolSensorData typedData = new BoolSensorData() { BoolValue = sensorValue.BoolValue, Comment = sensorValue.Comment };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public SensorDataObject ConvertToDatabase(IntSensorValue sensorValue, DateTime timeCollected)
        {
            FillCommonFields(sensorValue, timeCollected, out var result);
            result.DataType = SensorType.IntSensor;
            result.Status = sensorValue.Status;

            IntSensorData typedData = new IntSensorData() { IntValue = sensorValue.IntValue, Comment = sensorValue.Comment };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public SensorDataObject ConvertToDatabase(DoubleSensorValue sensorValue, DateTime timeCollected)
        {
            FillCommonFields(sensorValue, timeCollected, out var result);
            result.DataType = SensorType.DoubleSensor;
            result.Status = sensorValue.Status;

            DoubleSensorData typedData = new DoubleSensorData() { DoubleValue = sensorValue.DoubleValue, Comment = sensorValue.Comment };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public SensorDataObject ConvertToDatabase(StringSensorValue sensorValue, DateTime timeCollected)
        {
            FillCommonFields(sensorValue, timeCollected, out var result);
            result.DataType = SensorType.StringSensor;
            result.Status = sensorValue.Status;

            StringSensorData typedData = new StringSensorData() { StringValue = sensorValue.StringValue, Comment = sensorValue.Comment };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public SensorDataObject ConvertToDatabase(FileSensorValue sensorValue, DateTime timeCollected)
        {
            FillCommonFields(sensorValue, timeCollected, out var result);
            result.DataType = SensorType.FileSensor;
            result.Status = sensorValue.Status;

            FileSensorData typedData = new FileSensorData()
            {
                Comment = sensorValue.Comment, Extension = sensorValue.Extension, FileContent = sensorValue.FileContent, FileName = sensorValue.FileName
            };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;

        }

        public SensorDataObject ConvertToDatabase(FileSensorBytesValue sensorValue, DateTime timeCollected)
        {
            FillCommonFields(sensorValue, timeCollected, out var result);
            result.DataType = SensorType.FileSensor;
            result.Status = sensorValue.Status;

            FileSensorBytesData typedData = new FileSensorBytesData()
            {
                Comment = sensorValue.Comment,
                Extension = sensorValue.Extension,
                FileContent = sensorValue.FileContent,
                FileName = sensorValue.FileName
            };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }
        public SensorDataObject ConvertToDatabase(IntBarSensorValue sensorValue, DateTime timeCollected)
        {
            FillCommonFields(sensorValue, timeCollected, out var result);
            result.DataType = SensorType.IntegerBarSensor;

            IntBarSensorData typedData = ToTypedData(sensorValue);
            result.TypedData = JsonSerializer.Serialize(typedData);
            result.Status = sensorValue.Status;
            return result;
        }

        public SensorDataObject ConvertToDatabase(DoubleBarSensorValue sensorValue, DateTime timeCollected)
        {
            FillCommonFields(sensorValue, timeCollected, out var result);
            result.DataType = SensorType.DoubleBarSensor;

            DoubleBarSensorData typedData = ToTypedData(sensorValue);
            result.TypedData = JsonSerializer.Serialize(typedData);
            result.Status = sensorValue.Status;
            return result;
        }


        private IntBarSensorData ToTypedData(IntBarSensorValue sensorValue)
        {
            IntBarSensorData typedData = new IntBarSensorData()
            {
                Max = sensorValue.Max,
                Min = sensorValue.Min,
                Mean = sensorValue.Mean,
                Count = sensorValue.Count,
                Comment = sensorValue.Comment,
                StartTime = sensorValue.StartTime.ToUniversalTime(),
                EndTime = sensorValue.EndTime.ToUniversalTime(),
                Percentiles = sensorValue.Percentiles,
                LastValue = sensorValue.LastValue
            };
            return typedData;
        }

        private DoubleBarSensorData ToTypedData(DoubleBarSensorValue sensorValue)
        {
            DoubleBarSensorData typedData = new DoubleBarSensorData()
            {
                Max = sensorValue.Max,
                Min = sensorValue.Min,
                Mean = sensorValue.Mean,
                Count = sensorValue.Count,
                Comment = sensorValue.Comment,
                StartTime = sensorValue.StartTime.ToUniversalTime(),
                EndTime = sensorValue.EndTime.ToUniversalTime(),
                Percentiles = sensorValue.Percentiles,
                LastValue = sensorValue.LastValue
            };
            return typedData;
        }
        #endregion
        
        #region Independent update messages

        public SensorData Convert(SensorDataObject dataObject, SensorInfo sensorInfo, string productName)
        {
            var converted = Convert(dataObject, productName);
            converted.Description = sensorInfo.Description;
            return converted;
        }
        public SensorData Convert(SensorDataObject dataObject, string productName)
        {
            SensorData result = new SensorData();
            result.Path = dataObject.Path;
            result.SensorType = dataObject.DataType;
            result.Product = productName;
            result.Time = dataObject.TimeCollected;
            result.StringValue = GetStringValue(dataObject.TypedData, dataObject.DataType, dataObject.TimeCollected);
            result.ShortStringValue = GetShortStringValue(dataObject.TypedData, dataObject.DataType);
            result.Status = dataObject.Status;
            return result;
        }

        public SensorData Convert(BoolSensorValue value, string productName, DateTime timeCollected, TransactionType type)
        {
            AddCommonValues(value, productName, timeCollected, type, out var data);
            data.StringValue = GetStringValue(value, timeCollected);
            data.ShortStringValue = GetShortStringValue(value);
            data.SensorType = SensorType.BooleanSensor;
            return data;
        }

        public SensorData Convert(IntSensorValue value, string productName, DateTime timeCollected, TransactionType type)
        {
            AddCommonValues(value, productName, timeCollected, type, out var data);
            data.StringValue = GetStringValue(value, timeCollected);
            data.ShortStringValue = GetShortStringValue(value);
            data.SensorType = SensorType.IntSensor;
            return data;
        }

        public SensorData Convert(DoubleSensorValue value, string productName, DateTime timeCollected, TransactionType type)
        {
            AddCommonValues(value, productName, timeCollected, type, out var data);
            data.StringValue = GetStringValue(value, timeCollected);
            data.ShortStringValue = GetShortStringValue(value);
            data.SensorType = SensorType.DoubleSensor;
            return data;
        }

        public SensorData Convert(StringSensorValue value, string productName, DateTime timeCollected, TransactionType type)
        {
            AddCommonValues(value, productName, timeCollected, type, out var data);
            data.StringValue = GetStringValue(value, timeCollected);
            data.ShortStringValue = GetShortStringValue(value);
            data.SensorType = SensorType.StringSensor;
            return data;
        }

        public SensorData Convert(FileSensorValue value, string productName, DateTime timeCollected, TransactionType type)
        {
            AddCommonValues(value, productName, timeCollected, type, out var data);
            data.StringValue = GetStringValue(value, timeCollected);
            data.ShortStringValue = GetShortStringValue(value);
            data.SensorType = SensorType.FileSensor;
            return data;
        }

        public SensorData Convert(FileSensorBytesValue value, string productName, DateTime timeCollected,
            TransactionType type)
        {
            AddCommonValues(value, productName, timeCollected, type, out var data);
            data.StringValue = GetStringValue(value, timeCollected);
            data.ShortStringValue = GetShortStringValue(value);
            data.SensorType = SensorType.FileSensorBytes;
            return data;
        }
        public SensorData Convert(IntBarSensorValue value, string productName, DateTime timeCollected, TransactionType type)
        {
            AddCommonValues(value, productName, timeCollected, type, out var data);
            data.StringValue = GetStringValue(value, timeCollected);
            data.ShortStringValue = GetShortStringValue(value);
            data.SensorType = SensorType.IntegerBarSensor;
            return data;
        }
        public SensorData Convert(DoubleBarSensorValue value, string productName, DateTime timeCollected, TransactionType type)
        {
            AddCommonValues(value, productName, timeCollected, type, out var data);
            data.StringValue = GetStringValue(value, timeCollected);
            data.ShortStringValue = GetShortStringValue(value);
            data.SensorType = SensorType.DoubleBarSensor;
            return data;
        }
        private void AddCommonValues(SensorValueBase value, string productName, DateTime timeCollected, TransactionType type, out SensorData data)
        {
            data = new SensorData();
            data.Path = value.Path;
            data.Product = productName;
            data.Time = timeCollected;
            data.TransactionType = type;
            data.Description = value.Description;
            data.Status = value.Status;
            data.Key = value.Key;
        }

        #endregion
        #region Typed data objects

        private string GetStringValue(string stringData, SensorType sensorType, DateTime timeCollected)
        {
            string result = string.Empty;
            switch (sensorType)
            {
                case SensorType.BooleanSensor:
                    {
                        try
                        {
                            BoolSensorData boolData = JsonSerializer.Deserialize<BoolSensorData>(stringData);
                            result = !string.IsNullOrEmpty(boolData.Comment)
                                ? $"Time: {timeCollected.ToUniversalTime():G}. Value = {boolData.BoolValue}, comment = {boolData.Comment}"
                                : $"Time: {timeCollected.ToUniversalTime():G}. Value = {boolData.BoolValue}.";
                            result = $"{boolData.BoolValue}";
                        }
                        catch { }
                        break;
                    }
                case SensorType.IntSensor:
                    {
                        try
                        {
                            IntSensorData intData = JsonSerializer.Deserialize<IntSensorData>(stringData);
                            result = !string.IsNullOrEmpty(intData.Comment)
                                ? $"Time: {timeCollected.ToUniversalTime():G}. Value = {intData.IntValue}, comment = {intData.Comment}"
                                : $"Time: {timeCollected.ToUniversalTime():G}. Value = {intData.IntValue}.";
                        }
                        catch { }
                        break;
                    }
                case SensorType.DoubleSensor:
                    {
                        try
                        {
                            DoubleSensorData doubleData = JsonSerializer.Deserialize<DoubleSensorData>(stringData);
                            result = !string.IsNullOrEmpty(doubleData.Comment)
                                ? $"Time: {timeCollected.ToUniversalTime():G}. Value = {doubleData.DoubleValue}, comment = {doubleData.Comment}"
                                : $"Time: {timeCollected.ToUniversalTime():G}. Value = {doubleData.DoubleValue}.";
                        }
                        catch { }
                        break;
                    }
                case SensorType.StringSensor:
                    {
                        try
                        {
                            StringSensorData stringTypedData = JsonSerializer.Deserialize<StringSensorData>(stringData);
                            result = !string.IsNullOrEmpty(stringTypedData.Comment)
                                ? $"Time: {timeCollected.ToUniversalTime():G}. Value = '{stringTypedData.StringValue}', comment = {stringTypedData.Comment}"
                                : $"Time: {timeCollected.ToUniversalTime():G}. Value = '{stringTypedData.StringValue}'.";
                        }
                        catch { }
                        break;
                    }
                case SensorType.IntegerBarSensor:
                    {
                        try
                        {
                            IntBarSensorData intBarData = JsonSerializer.Deserialize<IntBarSensorData>(stringData);
                            result = !string.IsNullOrEmpty(intBarData.Comment)
                                ? $"Time: {timeCollected.ToUniversalTime():G}. Value: Min = {intBarData.Min}, Mean = {intBarData.Mean}, Max = {intBarData.Max}, Count = {intBarData.Count}, Last = {intBarData.LastValue}. Comment = {intBarData.Comment}"
                                : $"Time: {timeCollected.ToUniversalTime():G}. Value: Min = {intBarData.Min}, Mean = {intBarData.Mean}, Max = {intBarData.Max}, Count = {intBarData.Count}, Last = {intBarData.LastValue}.";
                        }
                        catch { }
                        break;
                    }
                case SensorType.DoubleBarSensor:
                    {
                        try
                        {
                            DoubleBarSensorData doubleBarData = JsonSerializer.Deserialize<DoubleBarSensorData>(stringData);
                            result = !string.IsNullOrEmpty(doubleBarData.Comment)
                                ? $"Time: {timeCollected.ToUniversalTime():G}. Value: Min = {doubleBarData.Min}, Mean = {doubleBarData.Mean}, Max = {doubleBarData.Max}, Count = {doubleBarData.Count}, Last = {doubleBarData.LastValue}. Comment = {doubleBarData.Comment}"
                                : $"Time: {timeCollected.ToUniversalTime():G}. Value: Min = {doubleBarData.Min}, Mean = {doubleBarData.Mean}, Max = {doubleBarData.Max}, Count = {doubleBarData.Count}, Last = {doubleBarData.LastValue}.";
                        }
                        catch { }
                        break;
                    }
                case SensorType.FileSensor:
                    {
                        try
                        {
                            FileSensorData fileData = JsonSerializer.Deserialize<FileSensorData>(stringData);
                            string sizeString = FileSizeToNormalString(fileData?.FileContent?.Length ?? 0);
                            string fileNameString = GetFileNameString(fileData.FileName, fileData.Extension);
                            result = !string.IsNullOrEmpty(fileData.Comment)
                                ? $"Time: {timeCollected.ToUniversalTime():G}. File size: {sizeString}. {fileNameString} Comment = {fileData.Comment}."
                                : $"Time: {timeCollected.ToUniversalTime():G}. File size: {sizeString}. {fileNameString}";
                        }
                        catch { }
                        break;
                    }
                case SensorType.FileSensorBytes:
                    {
                    try
                    {
                        FileSensorData fileData = JsonSerializer.Deserialize<FileSensorData>(stringData);
                        string sizeString = FileSizeToNormalString(fileData?.FileContent?.Length ?? 0);
                        string fileNameString = GetFileNameString(fileData.FileName, fileData.Extension);
                        result = !string.IsNullOrEmpty(fileData.Comment)
                            ? $"Time: {timeCollected.ToUniversalTime():G}. File size: {sizeString}. {fileNameString} Comment = {fileData.Comment}."
                            : $"Time: {timeCollected.ToUniversalTime():G}. File size: {sizeString}. {fileNameString}";
                    }
                    catch { }
                    break;
                    }
                default:
                {
                    result = string.Empty;
                    break;
                }
            }
            return result;
        }

        private string GetShortStringValue(string stringData, SensorType sensorType)
        {
            string result = string.Empty;
            switch (sensorType)
            {
                case SensorType.BooleanSensor:
                    {
                        try
                        {
                            BoolSensorData boolData = JsonSerializer.Deserialize<BoolSensorData>(stringData);
                            result = boolData.BoolValue.ToString();
                        }
                        catch { }
                        break;
                    }
                case SensorType.IntSensor:
                    {
                        try
                        {
                            IntSensorData intData = JsonSerializer.Deserialize<IntSensorData>(stringData);
                            result = intData.IntValue.ToString();
                        }
                        catch { }
                        break;
                    }
                case SensorType.DoubleSensor:
                    {
                        try
                        {
                            DoubleSensorData doubleData = JsonSerializer.Deserialize<DoubleSensorData>(stringData);
                            result = doubleData.DoubleValue.ToString();
                        }
                        catch { }
                        break;
                    }
                case SensorType.StringSensor:
                    {
                        try
                        {
                            StringSensorData stringTypedData = JsonSerializer.Deserialize<StringSensorData>(stringData);
                            result = stringTypedData.StringValue;
                        }
                        catch { }
                        break;
                    }
                case SensorType.IntegerBarSensor:
                    {
                        try
                        {
                            IntBarSensorData intBarData = JsonSerializer.Deserialize<IntBarSensorData>(stringData);
                            result =
                                $"Min = {intBarData.Min}, Mean = {intBarData.Mean}, Max = {intBarData.Max}, Count = {intBarData.Count}, Last = {intBarData.LastValue}.";
                        }
                        catch { }
                        break;
                    }
                case SensorType.DoubleBarSensor:
                    {
                        try
                        {
                            DoubleBarSensorData doubleBarData = JsonSerializer.Deserialize<DoubleBarSensorData>(stringData);
                            result =
                                $"Min = {doubleBarData.Min}, Mean = {doubleBarData.Mean}, Max = {doubleBarData.Max}, Count = {doubleBarData.Count}, Last = {doubleBarData.LastValue}.";
                        }
                        catch { }
                        break;
                    }
                case SensorType.FileSensor:
                    {
                        try
                        {
                            FileSensorData fileData = JsonSerializer.Deserialize<FileSensorData>(stringData);
                            string sizeString = FileSizeToNormalString(fileData?.FileContent?.Length ?? 0);
                            string fileNameString = GetFileNameString(fileData.FileName, fileData.Extension);
                            result = $"File size: {sizeString}. {fileNameString}";
                        }
                        catch { }
                        break;
                    }
                case SensorType.FileSensorBytes:
                    {
                        try
                        {
                            FileSensorData fileData = JsonSerializer.Deserialize<FileSensorData>(stringData);
                            string sizeString = FileSizeToNormalString(fileData?.FileContent?.Length ?? 0);
                            string fileNameString = GetFileNameString(fileData.FileName, fileData.Extension);
                            result = $"File size: {sizeString}. {fileNameString}";
                        }
                        catch { }
                        break;
                    }
                default:
                    {
                        result = string.Empty;
                        break;
                    }
            }
            return result;
        }
        private string GetStringValue(BoolSensorValue value, DateTime timeCollected)
        {
            string result = string.Empty;
            try
            {
                result = !string.IsNullOrEmpty(value.Comment)
                    ? $"Time: {timeCollected.ToUniversalTime():G}. Value = {value.BoolValue}, comment = {value.Comment}."
                    : $"Time: {timeCollected.ToUniversalTime():G}. Value = {value.BoolValue}.";
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Failed to get short value");
            }

            return result;
        }

        private string GetStringValue(IntSensorValue value, DateTime timeCollected)
        {
            string result = string.Empty;
            try
            {
                result = !string.IsNullOrEmpty(value.Comment)
                    ? $"Time: {timeCollected.ToUniversalTime():G}. Value = {value.IntValue}, comment = {value.Comment}."
                    : $"Time: {timeCollected.ToUniversalTime():G}. Value = {value.IntValue}.";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }

        private string GetStringValue(DoubleSensorValue value, DateTime timeCollected)
        {
            string result = string.Empty;
            try
            {
                result = !string.IsNullOrEmpty(value.Comment)
                    ? $"Time: {timeCollected.ToUniversalTime():G}. Value = {value.DoubleValue}, comment = {value.Comment}."
                    : $"Time: {timeCollected.ToUniversalTime():G}. Value = {value.DoubleValue}.";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }

        private string GetStringValue(StringSensorValue value, DateTime timeCollected)
        {
            string result = string.Empty;
            try
            {
                result = !string.IsNullOrEmpty(value.Comment)
                    ? $"Time: {timeCollected.ToUniversalTime():G}. Value = {value.StringValue}, comment = {value.Comment}."
                    : $"Time: {timeCollected.ToUniversalTime():G}. Value = {value.StringValue}.";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }

        private string GetStringValue(FileSensorValue value, DateTime timeCollected)
        {
            string result = string.Empty;
            try
            {
                string sizeString = FileSizeToNormalString(value?.FileContent?.Length ?? 0);
                string fileNameString = GetFileNameString(value.FileName, value.Extension);
                result = !string.IsNullOrEmpty(value.Comment)
                    ? $"Time: {timeCollected.ToUniversalTime():G}. File size: {sizeString}. {fileNameString} Comment = {value.Comment}."
                    : $"Time: {timeCollected.ToUniversalTime():G}. File size: {sizeString}. {fileNameString}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }
        private string GetStringValue(FileSensorBytesValue value, DateTime timeCollected)
        {
            string result = string.Empty;
            try
            {
                string sizeString = FileSizeToNormalString(value?.FileContent?.Length ?? 0);
                string fileNameString = GetFileNameString(value.FileName, value.Extension);
                result = !string.IsNullOrEmpty(value.Comment)
                    ? $"Time: {timeCollected.ToUniversalTime():G}. File size: {sizeString}. {fileNameString} Comment = {value.Comment}."
                    : $"Time: {timeCollected.ToUniversalTime():G}. File size: {sizeString}. {fileNameString}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }
        private string GetStringValue(IntBarSensorValue value, DateTime timeCollected)
        {
            string result = string.Empty;
            try
            {
                result = !string.IsNullOrEmpty(value.Comment)
                    ? $"Time: {timeCollected.ToUniversalTime():G}. Value: Min = {value.Min}, Mean = {value.Mean}, Max = {value.Max}, Count = {value.Count}, Last = {value.LastValue}. Comment = {value.Comment}."
                    : $"Time: {timeCollected.ToUniversalTime():G}. Value: Min = {value.Min}, Mean = {value.Mean}, Max = {value.Max}, Count = {value.Count}, Last = {value.LastValue}.";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }

        private string GetStringValue(DoubleBarSensorValue value, DateTime timeCollected)
        {
            string result = string.Empty;
            try
            {
                result = !string.IsNullOrEmpty(value.Comment)
                    ? $"Time: {timeCollected.ToUniversalTime():G}. Value: Min = {value.Min}, Mean = {value.Mean}, Max = {value.Max}, Count = {value.Count}, Last = {value.LastValue}. Comment = {value.Comment}."
                    : $"Time: {timeCollected.ToUniversalTime():G}. Value: Min = {value.Min}, Mean = {value.Mean}, Max = {value.Max}, Count = {value.Count}, Last = {value.LastValue}.";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }

        private string GetShortStringValue(BoolSensorValue value)
        {
            return value.BoolValue.ToString();
        }

        private string GetShortStringValue(IntSensorValue value)
        {
            return value.IntValue.ToString();
        }

        private string GetShortStringValue(DoubleSensorValue value)
        {
            return value.DoubleValue.ToString();
        }

        private string GetShortStringValue(StringSensorValue value)
        {
            return value.StringValue;
        }

        private string GetShortStringValue(IntBarSensorValue value)
        {
            string result = string.Empty;
            try
            {
                result =
                    $"Min = {value.Min}, Mean = {value.Mean}, Max = {value.Max}, Count = {value.Count}, Last = {value.LastValue}.";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }
            return result;
        }

        private string GetShortStringValue(DoubleBarSensorValue value)
        {
            string result = string.Empty;
            try
            {
                result =
                    $"Min = {value.Min}, Mean = {value.Mean}, Max = {value.Max}, Count = {value.Count}, Last = {value.LastValue}.";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }
            return result;
        }

        private string GetShortStringValue(FileSensorValue value)
        {
            string result = string.Empty;
            try
            {
                string sizeString = FileSizeToNormalString(value?.FileContent?.Length ?? 0);
                string fileNameString = GetFileNameString(value.FileName, value.Extension);
                result = $"File size: {sizeString}. {fileNameString}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }

        private string GetShortStringValue(FileSensorBytesValue value)
        {
            string result = string.Empty;
            try
            {
                string sizeString = FileSizeToNormalString(value?.FileContent?.Length ?? 0);
                string fileNameString = GetFileNameString(value.FileName, value.Extension);
                result = $"File size: {sizeString}. {fileNameString}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get short value");
            }

            return result;
        }
        #endregion

        public SensorInfo Convert(string productName, string path)
        {
            SensorInfo result = new SensorInfo();
            result.Path = path;
            result.ProductName = productName;
            result.SensorName = ExtractSensor(path);
            return result;
        }
        public SensorInfo Convert(string productName, SensorValueBase sensorValue)
        {
            SensorInfo result = new SensorInfo();
            result.Path = sensorValue.Path;
            result.Description = sensorValue.Description;
            result.ProductName = productName;
            result.SensorName = ExtractSensor(sensorValue.Path);
            return result;
        }
        //public ProductDataMessage Convert(Product product)
        //{
        //    ProductDataMessage result = new ProductDataMessage();
        //    result.Name = product.Name;
        //    result.Key = product.Key;
        //    result.DateAdded = product.DateAdded.ToUniversalTime().ToTimestamp();
        //    return result;
        //}

        //public GenerateClientCertificateModel Convert(CertificateRequestMessage requestMessage)
        //{
        //    GenerateClientCertificateModel model = new GenerateClientCertificateModel
        //    {
        //        CommonName = requestMessage.CommonName,
        //        CountryName = requestMessage.CountryName,
        //        EmailAddress = requestMessage.EmailAddress,
        //        LocalityName = requestMessage.LocalityName,
        //        OrganizationName = requestMessage.OrganizationName,
        //        OrganizationUnitName = requestMessage.OrganizationUnitName,
        //        StateOrProvinceName = requestMessage.StateOrProvinceName
        //    };
        //    return model;
        //}

        //public RSAParameters Convert(HSMService.RSAParameters rsaParameters)
        //{
        //    RSAParameters result = new RSAParameters();
        //    result.D = rsaParameters.D.ToByteArray();
        //    result.DP = rsaParameters.DP.ToByteArray();
        //    result.DQ = rsaParameters.DQ.ToByteArray();
        //    result.Exponent = rsaParameters.Exponent.ToByteArray();
        //    result.InverseQ = rsaParameters.InverseQ.ToByteArray();
        //    result.Modulus = rsaParameters.Modulus.ToByteArray();
        //    result.P = rsaParameters.P.ToByteArray();
        //    result.Q = rsaParameters.Q.ToByteArray();
        //    return result;
        //}

        #region Sub-methods

        //private SensorObjectType Convert(SensorType type)
        //{
        //    //return (SensorObjectType) ((int) type);
        //    switch (type)
        //    {
        //        case SensorType.BooleanSensor:
        //            return SensorObjectType.ObjectTypeBoolSensor;
        //        case SensorType.DoubleSensor:
        //            return SensorObjectType.ObjectTypeDoubleSensor;
        //        case SensorType.IntSensor:
        //            return SensorObjectType.ObjectTypeIntSensor;
        //        case SensorType.StringSensor:
        //            return SensorObjectType.ObjectTypeStringSensor;
        //        case SensorType.IntegerBarSensor:
        //            return SensorObjectType.ObjectTypeBarIntSensor;
        //        case SensorType.DoubleBarSensor:
        //            return SensorObjectType.ObjectTypeBarDoubleSensor;
        //        case SensorType.FileSensor:
        //            return SensorObjectType.ObjectTypeFileSensor;
        //    }
        //    throw new Exception($"Unknown SensorDataType = {type}!");
        //}
        private long GetTimestamp(DateTime dateTime)
        {
            var timeSpan = (dateTime - DateTime.UnixEpoch);
            return (long)timeSpan.TotalSeconds;
        }

        public void ExtractProductAndSensor(string path, out string server, out string sensor)
        {
            server = string.Empty;
            sensor = string.Empty;
            var splitRes = path.Split("/".ToCharArray());
            server = splitRes[0];
            sensor = splitRes[^1];
        }

        public string ExtractSensor(string path)
        {
            var splitRes = path.Split("/".ToCharArray());
            return splitRes[^1];
        }

        private string GetFileNameString(string fileName, string extension)
        {
            if (string.IsNullOrEmpty(extension) && string.IsNullOrEmpty(fileName))
            {
                return "No file info specified!";
            }
            if (string.IsNullOrEmpty(fileName))
            {
                return $"Extension: {extension}.";
            }

            if (fileName.IndexOf('.') != -1)
            {
                return $"File name: {fileName}.";
            }

            return $"File name: {fileName}.{extension}.";
        }
        private string FileSizeToNormalString(int size)
        {
            if (size < SIZE_DENOMINATOR)
            {
                return $"{size} bytes";
            }

            double kb = size / SIZE_DENOMINATOR;
            if (kb < SIZE_DENOMINATOR)
            {
                return $"{kb:#,##0} KB";
            }

            double mb = kb / SIZE_DENOMINATOR;
            if (mb < SIZE_DENOMINATOR)
            {
                return $"{mb:#,##0.0} MB";
            }

            double gb = mb / SIZE_DENOMINATOR;
            return $"{gb:#,##0.0} GB";
        }
        #endregion

    }
}
