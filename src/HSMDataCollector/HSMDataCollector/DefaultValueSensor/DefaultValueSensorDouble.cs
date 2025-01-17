﻿using System;
//using System.Text.Json;
using HSMDataCollector.Core;
using HSMDataCollector.PublicInterface;
using HSMSensorDataObjects;
using HSMSensorDataObjects.FullDataObject;
using HSMSensorDataObjects.TypedDataObject;
using Newtonsoft.Json;

namespace HSMDataCollector.DefaultValueSensor
{
    internal class DefaultValueSensorDouble : DefaultValueSensorBase<double>, IDefaultValueSensorDouble
    {
        public DefaultValueSensorDouble(string path, string productKey, IValuesQueue queue, double defaultValue) : base(path, productKey, queue, defaultValue)
        {
        }

        public override CommonSensorValue GetLastValue()
        {
            CommonSensorValue commonSensorValue = new CommonSensorValue();
            commonSensorValue.SensorType = SensorType.IntSensor;
            commonSensorValue.TypedValue = JsonConvert.SerializeObject(GetValue());
            return commonSensorValue;
        }

        public void AddValue(double value)
        {
            lock (_syncRoot)
            {
                _currentValue = value;
            }
        }

        public void AddValue(double value, string comment)
        {
            lock (_syncRoot)
            {
                _currentValue = value;
                _currentComment = comment;
            }
        }

        public void AddValue(double value, SensorStatus status, string comment = null)
        {
            lock (_syncRoot)
            {
                _currentValue = value;
                _currentStatus = status;
                _currentComment = comment;
            }
        }

        private DoubleSensorValue GetValue()
        {
            double val;
            string comment;
            SensorStatus status;
            lock (_syncRoot)
            {
                val = _currentValue;
                comment = _currentComment;
                status = _currentStatus;
            }

            DoubleSensorValue result = new DoubleSensorValue() { DoubleValue = val, Key = ProductKey, Path = Path, Time = DateTime.Now, Comment = comment, Status = status};
            return result;
        }
    }
}
