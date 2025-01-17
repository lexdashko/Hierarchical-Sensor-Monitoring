﻿using HSMServer.Authentication;
using HSMServer.Configuration;
using HSMServer.DataLayer.Model;
using HSMServer.Extensions;
using LevelDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace HSMServer.DataLayer
{
    public class LevelDBDatabaseClass : IDatabaseClass
    {
        #region IDisposable implementation

        private bool _disposed;

        // Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedResources)
        {
            // The idea here is that Dispose(Boolean) knows whether it is 
            // being called to do explicit cleanup (the Boolean is true) 
            // versus being called due to a garbage collection (the Boolean 
            // is false). This distinction is useful because, when being 
            // disposed explicitly, the Dispose(Boolean) method can safely 
            // execute code using reference type fields that refer to other 
            // objects knowing for sure that these other objects have not been 
            // finalized or disposed of yet. When the Boolean is false, 
            // the Dispose(Boolean) method should not execute code that 
            // refer to reference type fields because those objects may 
            // have already been finalized."

            if (!_disposed)
            {
                if (disposingManagedResources)
                {
                    // Dispose managed resources here...
                    //foreach (var counter in Counters)
                    //  counter.Dispose();
                    _database.Dispose();
                    _database = null;
                }

                // Dispose unmanaged resources here...

                // Set large fields to null here...

                // Mark as disposed.
                _disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~LevelDBDatabaseClass()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #endregion

        private readonly object _accessLock;
        private readonly ILogger<LevelDBDatabaseClass> _logger;
        private readonly char[] _keysSeparator = { '_' };
        private const string DATABASE_NAME = "MonitoringData";
        private readonly Options _dbOptions = new Options() { CreateIfMissing = true, MaxOpenFiles = 100000 };
        private DB _database;
        private string _currentDatabaseName = DATABASE_NAME;
        public LevelDBDatabaseClass(ILogger<LevelDBDatabaseClass> logger)
        {
            _accessLock = new object();
            _logger = logger;
            try
            {
                _database = new DB(_dbOptions, DATABASE_NAME, Encoding.UTF8);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create LevelDB database");
                throw;
            }
            
        }


        #region Management

        public void CloseDatabase()
        {
            try
            {
                lock (_accessLock)
                {
                    _database.Close();
                    _database.Dispose();
                    _database = null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to close the database", e);
            }
            
        }

        public void OpenDatabase(string databaseName)
        {
            try
            {
                lock (_accessLock)
                {
                    //Required Db is already opened
                    if (_database?.PropertyValue("name") == databaseName)
                        return;

                    if (_database != null)
                    {
                        _database.Close();
                        _database.Dispose();
                        _database = null;
                    }

                    _database = new DB(_dbOptions, databaseName, Encoding.UTF8);
                    _currentDatabaseName = databaseName;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to open new database", e);
            }
            
        }

        public void DeleteDatabase()
        {
            CloseDatabase();
            try
            {
                Directory.Delete(_currentDatabaseName, true);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to delete database directory", e);
            }
        }

        #endregion

        #region Product

        public void AddProductToList(string productName)
        {
            try
            {
                lock (_accessLock)
                {
                    var currentValue = _database.Get(PrefixConstants.PRODUCTS_LIST_PREFIX);
                    _database.Delete(PrefixConstants.PRODUCTS_LIST_PREFIX);
                    var prodList = string.IsNullOrEmpty(currentValue)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(currentValue);
                    _logger.LogInformation($"Products list read: {currentValue}");
                    prodList.Add(productName);
                    _database.Put(PrefixConstants.PRODUCTS_LIST_PREFIX, JsonSerializer.Serialize(prodList));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add product to list");
                throw;
            }
        }

        public List<string> GetProductsList()
        {
            List<string> result = new List<string>();
            try
            {
                lock (_accessLock)
                {
                    var value = _database.Get(PrefixConstants.PRODUCTS_LIST_PREFIX);
                    result.AddRange(JsonSerializer.Deserialize<List<string>>(value));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get products list");
            }

            return result;
        }

        public Product GetProductInfo(string productName)
        {
            Product result = default(Product);
            try
            {
                lock (_accessLock)
                {
                    var value = _database.Get(GetProductInfoKey(productName));
                    result = JsonSerializer.Deserialize<Product>(value);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get product info for product = {productName}");
            }

            return result;
        }

        public void PutProductInfo(Product product)
        {
            try
            {
                string key = GetProductInfoKey(product.Name);
                string value = JsonSerializer.Serialize(product);
                lock (_accessLock)
                {
                    _database.Put(key, value);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add product info");
            }
        }

        public void RemoveProductInfo(string name)
        {
            try
            {
                string key = GetProductInfoKey(name);
                lock (_accessLock)
                {
                    _database.Delete(key);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to remove product info for product = {name}");
            }
        }

        public void RemoveProductFromList(string name)
        {
            try
            {
                lock (_accessLock)
                {
                    var currentValue = _database.Get(PrefixConstants.PRODUCTS_LIST_PREFIX);
                    _database.Delete(PrefixConstants.PRODUCTS_LIST_PREFIX);
                    List<string> list = JsonSerializer.Deserialize<List<string>>(currentValue);
                    list.Remove(name);
                    _database.Put(PrefixConstants.PRODUCTS_LIST_PREFIX, JsonSerializer.Serialize(list));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failer to remove prodcut from list");
            }
        }

        #endregion

        #region Sensors

        public void RemoveSensor(SensorInfo info)
        {
            try
            {
                string key = GetSensorInfoKey(info);
                lock (_accessLock)
                {
                    _database.Delete(key);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to remove sensor info for {info.Path}");
            }
        }

        public void AddSensor(SensorInfo info)
        {
            try
            {
                string key = GetSensorInfoKey(info);
                string value = JsonSerializer.Serialize(info);
                lock (_accessLock)
                {
                    _database.Put(key, value);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to add sensor info for {info.Path}");
            }
        }
        public SensorInfo GetSensorInfo(string productName, string path)
        {
            SensorInfo sensorInfo = default(SensorInfo);
            try
            {
                string key = GetSensorInfoKey(productName, path);
                string value;
                lock (_accessLock)
                {
                    value = _database.Get(key);
                }

                sensorInfo = JsonSerializer.Deserialize<SensorInfo>(value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to read SensorInfo for {productName}:{path}");
            }

            return sensorInfo;
        }

        public void RemoveSensorValues(string productName, string path)
        {
            try
            {
                string readKey = GetSensorReadValueKey(productName, path);
                byte[] bytesKey = Encoding.UTF8.GetBytes(readKey);
                int count = 0;
                lock (_accessLock)
                {
                    using (var iterator = _database.CreateIterator())
                    {
                        for (iterator.Seek(bytesKey); iterator.IsValid() && iterator.Key().StartsWith(bytesKey);
                            iterator.Next())
                        {
                            _database.Delete(iterator.Key());
                            ++count;
                        }    
                    }
                
                }
                _logger.LogInformation($"Removed {count} values of sensor {path}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to remove values of sensors {path}");
            }
        }

        public void WriteSensorData(SensorDataObject dataObject, string productName)
        {
            try
            {
                var key = GetSensorWriteValueKey(productName, dataObject.Path, dataObject.TimeCollected);
                string value = JsonSerializer.Serialize(dataObject);
                lock (_accessLock)
                {
                    _database.Put(key, value);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to add data for sensor {dataObject.Path}");
            }
        }

        public void WriteOneValueSensorData(SensorDataObject dataObject, string productName)
        {
            try
            {
                var key = GetOneValueSensorWriteKey(productName, dataObject.Path);
                string value = JsonSerializer.Serialize(dataObject);
                lock (_accessLock)
                {
                    _database.Put(key, value);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to add data for sensor {dataObject.Path}");
            }
        }

        public SensorDataObject GetLastSensorValue(string productName, string path)
        {
            SensorDataObject sensorDataObject = default(SensorDataObject);
            try
            {
                byte[] searchKey = Encoding.UTF8.GetBytes(GetSensorReadValueKey(productName, path));
                DateTime lastDateTime = DateTime.MinValue;
                byte[] bytesValue = new byte[0];
                lock (_accessLock)
                {
                    using (var iterator = _database.CreateIterator())
                    {
                        //for (iterator.SeekToFirst(); iterator.IsValid(); iterator.Next())
                        //{
                        //    if (!iterator.Key().StartsWith(searchKey))
                        //        continue;

                        //    try
                        //    {
                        //        DateTime currentDateTime = GetTimeFromSensorWriteKey(iterator.Key());
                        //        if (currentDateTime > lastDateTime)
                        //        {
                        //            lastDateTime = currentDateTime;
                        //            bytesValue = iterator.Value();
                        //        }
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        _logger.LogError(e, "Failed to read SensorDataObject");
                        //    }

                        //}
                        for (iterator.Seek(searchKey); iterator.IsValid() && iterator.Key().StartsWith(searchKey); iterator.Next())
                        {
                            try
                            {
                                DateTime currentDateTime = GetTimeFromSensorWriteKey(iterator.Key());
                                if (currentDateTime > lastDateTime)
                                {
                                    lastDateTime = currentDateTime;
                                    bytesValue = iterator.Value();
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Failed to read SensorDataObject");
                            }
                        }
                    }
                }
                string stringValue = Encoding.UTF8.GetString(bytesValue);
                sensorDataObject = JsonSerializer.Deserialize<SensorDataObject>(stringValue);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get last value for sensor = {path}, product = {productName}");
            }

            return sensorDataObject;
        }

        public List<SensorDataObject> GetSensorDataHistory(string productName, string path, long n)
        {
            List<SensorDataObject> result = new List<SensorDataObject>();
            try
            {
                byte[] searchKey = Encoding.UTF8.GetBytes(GetSensorReadValueKey(productName, path));
                lock (_accessLock)
                {
                    using (var iterator = _database.CreateIterator())
                    {
                        for (iterator.SeekToFirst(); iterator.IsValid(); iterator.Next())
                        {
                            if (!iterator.Key().StartsWith(searchKey))
                                continue;

                            try
                            {
                                var typedValue = JsonSerializer.Deserialize<SensorDataObject>(iterator.ValueAsString());
                                if (typedValue.Path == path)
                                {
                                    result.Add(typedValue);
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Failed to read SensorDataObject");
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get sensor history {path}");
            }

            return result;
        }

        public List<string> GetSensorsList(string productName)
        {
            List<string> result = new List<string>();
            try
            {
                var key = GetSensorsListKey(productName);
                string value;
                lock (_accessLock)
                {
                    value = _database.Get(key);
                }
                result.AddRange(JsonSerializer.Deserialize<List<string>>(value));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get sensors list for {productName}");
            }

            return result;
        }

        public void AddNewSensorToList(string productName, string path)
        {
            try
            {
                string key = GetSensorsListKey(productName);
                lock (_accessLock)
                {
                    string currentValue = _database.Get(key);
                    _database.Delete(key);
                    var list = string.IsNullOrEmpty(currentValue)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(currentValue);
                    list.Add(path);
                    _database.Put(key, JsonSerializer.Serialize(list));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to add new sensor {path} to list for product {productName}");
            }
        }

        public void RemoveSensorFromList(string productName, string sensorName)
        {
            try
            {
                string key = GetSensorsListKey(productName);
                lock (_accessLock)
                {
                    var currentValue = _database.Get(key);
                    _database.Delete(key);
                    List<string> list = JsonSerializer.Deserialize<List<string>>(currentValue);
                    list.Remove(sensorName);
                    _database.Put(key, JsonSerializer.Serialize(list));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to remove sensor {sensorName} from list for {productName}");
            }
        }

        #endregion

        #region Configuration

        public void AddUser(User user)
        {
            try
            {
                string key = GetUniqueUserKey(user.UserName);
                string value = JsonSerializer.Serialize(user);
                lock (_accessLock)
                {
                    _database.Put(key, value);                 
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to write user data for user = '{user.UserName}'");
            }
        }

        public List<User> ReadUsersPage(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            int index = 1;
            var lastIndex = page * pageSize;
            List<User> users = new List<User>();
            try
            {
                byte[] searchKey = Encoding.UTF8.GetBytes(GetUserReadKey());
                lock (_accessLock)
                {
                    using (var iterator = _database.CreateIterator())
                    {
                        for (iterator.Seek(searchKey); iterator.IsValid() && iterator.Key().StartsWith(searchKey) &&
                                                       index <= lastIndex; iterator.Next(), ++index)
                        {
                            if (index <= skip)
                                continue;

                            try
                            {
                                User user = JsonSerializer.Deserialize<User>(iterator.ValueAsString());
                                users.Add(user);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"Failed to deserialize user from {iterator.ValueAsString()}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read ");
            }

            return users;
        }
        public List<User> ReadUsers()
        {
            List<User> users = new List<User>();
            try
            {
                byte[] searchKey = Encoding.UTF8.GetBytes(GetUserReadKey());
                lock (_accessLock)
                {
                    using (var iterator = _database.CreateIterator())
                    {
                        for (iterator.Seek(searchKey); iterator.IsValid() && iterator.Key().StartsWith(searchKey); iterator.Next())
                        {
                            try
                            {
                                User user = JsonSerializer.Deserialize<User>(iterator.ValueAsString());
                                users.Add(user);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"Failed to deserialize user from {iterator.ValueAsString()}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read ");
            }

            return users;
        }

        public void RemoveUser(User user)
        {
            try
            {
                string key = GetUniqueUserKey(user.UserName);
                lock (_accessLock)
                {
                    _database.Delete(key);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to remove user = '{user.UserName}'");
            }
        }

        public ConfigurationObject ReadConfigurationObject()
        {
            try
            {
                string key = GetConfigurationObjectKey();
                string value;
                lock (key)
                {
                    value = _database.Get(key);
                }

                return JsonSerializer.Deserialize<ConfigurationObject>(value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read ConfigurationObject!");
                return null;
            }
        }

        public void WriteConfigurationObject(ConfigurationObject obj)
        {
            try
            {
                string key = GetConfigurationObjectKey();
                string value = JsonSerializer.Serialize(obj);
                lock (_accessLock)
                {
                    _database.Delete(key);
                    _database.Put(key, value);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save ConfigurationObject!");
            }
        }

        #endregion

        #region Private methods

        private string GetConfigurationObjectKey()
        {
            return PrefixConstants.CONFIGURATION_OBJECT_PREFIX;
        }
        private string GetUniqueUserKey(string userName)
        {
            return $"{PrefixConstants.USER_INFO_PREFIX}_{userName}";
        }

        private string GetUserReadKey()
        {
            return PrefixConstants.USER_INFO_PREFIX;
        }
        private string GetSensorsListKey(string productName)
        {
            return $"{PrefixConstants.SENSORS_LIST_PREFIX}_{productName}";
        }
        private string GetSensorReadValueKey(string productName, string path)
        {
            return $"{PrefixConstants.SENSOR_VALUE_PREFIX}_{productName}_{path}";
        }

        private string GetOneValueSensorWriteKey(string productName, string path)
        {
            return $"{PrefixConstants.SENSOR_VALUE_PREFIX}_{productName}_{path}_{DateTime.MaxValue:G}_{DateTime.MaxValue.Ticks}";
        }
        private string GetSensorWriteValueKey(string productName, string path, DateTime putTime)
        {
            return
                $"{PrefixConstants.SENSOR_VALUE_PREFIX}_{productName}_{path}_{putTime:G}_{putTime.Ticks}";
        }
        private string GetSensorInfoKey(SensorInfo info)
        {
            return $"{PrefixConstants.SENSOR_KEY_PREFIX}_{info.ProductName}_{info.Path}";
        }

        private string GetSensorInfoKey(string productName, string path)
        {
            return $"{PrefixConstants.SENSOR_KEY_PREFIX}_{productName}_{path}";
        }
        private string GetProductInfoKey(string name)
        {
            return $"{PrefixConstants.PRODUCT_INFO_PREFIX}_{name}";
        }

        private DateTime GetTimeFromSensorWriteKey(byte[] keyBytes)
        {
            string str = Encoding.UTF8.GetString(keyBytes);
            var splitRes = str.Split(_keysSeparator);
            str = splitRes[^2];
            try
            {
                return DateTime.Parse(str);
            }
            catch (Exception e)
            {
                //_logger.LogError(e, $"LogError parsing datetime: {str}");
            }
            //Back compatibility
            str = splitRes.Last();
            try
            {
                return DateTime.Parse(str);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"LogError parsing datetime from prev version: {str}");
                return DateTime.MinValue;
            }
        }
        private long GetTimestamp(DateTime dateTime)
        {
            var timeSpan = (dateTime - DateTime.UnixEpoch);
            return (long)timeSpan.TotalSeconds;
        }

        private IEnumerable<string> ParseProducts(string serversListString)
        {
            List<string> result = new List<string>();
            string[] splitRes = serversListString.Split(";".ToCharArray());
            result.AddRange(splitRes.Select(srv => srv.Trim()));
            return result;
        }

        //private T GetTypedValue<T>(MDBValue value)
        //{
        //    byte[] bytes = value.CopyToNewArray();
        //    string stringVal = Encoding.UTF8.GetString(bytes);
        //    return JsonSerializer.Deserialize<T>(stringVal);
        //}

        #endregion
    }
}
