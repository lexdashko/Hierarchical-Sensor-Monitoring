﻿using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HSMClientWPFControls.ConnectorInterface;
using HSMClientWPFControls.Model;
using HSMClientWPFControls.Objects;
using HSMCommon.Model;

namespace HSMClient.Connection
{
    public abstract class ConnectorBase : IProductsConnector, ISensorsTreeConnector, ISensorHistoryConnector,
        ISettingsConnector
    {
        protected string _address;

        protected ConnectorBase(string address)
        {
            _address = address;
        }

        public abstract bool CheckServerAvailable();
        public abstract List<MonitoringSensorUpdate> GetTree();
        public abstract List<MonitoringSensorUpdate> GetUpdates();
        public abstract List<ProductInfo> GetProductsList();
        public abstract ProductInfo AddNewProduct(string name);
        public abstract bool RemoveProduct(string name);
        public abstract List<SensorHistoryItem> GetSensorHistory(string product, string path, string name, long n);
        public abstract byte[] GetFileSensorValueBytes(string product, string path);
        public abstract string GetFileSensorValueExtension(string product, string path);
        public abstract X509Certificate2 GetSignedClientCertificate(CreateCertificateModel model,
             out X509Certificate2 caCertificate);

        public abstract void ReplaceClientCertificate(X509Certificate2 certificate);
        public abstract ClientVersionModel GetLastAvailableVersion();
    }
}
