﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using HSMClient.Common;
using HSMClientWPFControls.Bases;
using HSMClientWPFControls.ViewModel;
using HSMSensorDataObjects;

namespace HSMClientWPFControls.Objects
{
    public class MonitoringNodeBase : NotifyingBase, IDisposable
    {
        #region IDisposable implementation

        // Disposed flag.
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
                    //foreach (var sensor in Sensors)
                    //  sensor.Dispose();
                    foreach (var subNode in SubNodes)
                        subNode.Dispose();
                }

                // Dispose unmanaged resources here...

                // Set large fields to null here...

                // Mark as disposed.
                _disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~MonitoringNodeBase()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #endregion

        private MonitoringNodeBase _parent;
        private string _name;
        private SensorStatus _status;
        private SensorStatus _internalStatus = SensorStatus.Unknown;
        private DateTime _lastStatusUpdate;
        private readonly Dictionary<string, MonitoringSensorViewModel> _nameToSensor;
        private readonly Dictionary<string, MonitoringNodeBase> _nameToNode;
        public MonitoringNodeBase(MonitoringNodeBase parent = null)
        {
            _parent = parent;
            _status = SensorStatus.Unknown;
            _lastStatusUpdate = DateTime.Now;
            SubNodes = new ObservableCollection<MonitoringNodeBase>();
            Sensors = new ObservableCollection<MonitoringSensorViewModel>();
            _nameToSensor = new Dictionary<string, MonitoringSensorViewModel>();
            _nameToNode = new Dictionary<string, MonitoringNodeBase>();
            SubNodes.CollectionChanged += Content_CollectionChanged;
            Sensors.CollectionChanged += Content_CollectionChanged;
        }

        private void Content_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged();
        }

        public MonitoringNodeBase(string name, MonitoringNodeBase parent = null) : this(parent)
        {
            _name = name;
        }

        //protected MonitoringNodeBase()
        //{

        //}
        public MonitoringNodeBase Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public SensorStatus InternalStatus
        {
            get => _internalStatus;
            set
            {
                _internalStatus = value;
                UpdateStatus();
            }
        }

        public SensorStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _lastStatusUpdate = DateTime.Now;
                }
                _status = value;
                _parent?.UpdateStatus();
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusString));
                OnPropertyChanged(nameof(StatusDuration));
            }
        }
        public string StatusString
        {
            get => TextConstants.ConvertStatus(_status);
            set
            {
                var convertedStatus = TextConstants.ConvertFromString(value);
                if (_status != convertedStatus)
                    _lastStatusUpdate = DateTime.Now;

                _status = convertedStatus;
                _parent.UpdateStatus();
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusString));
                OnPropertyChanged(nameof(StatusDuration));
            }
        }
        public virtual void UpdateStatus()
        {
            SensorStatus status = SensorStatus.Unknown;
            if (Sensors.Count > 0)
            {
                var sensorsStatus = Sensors.Max(s => s.Status);
                if (sensorsStatus > status)
                {
                    status = sensorsStatus;
                }
            }

            if (SubNodes.Count > 0)
            {
                var subNodesStatus = SubNodes.Max(n => n.Status);
                if (subNodesStatus > status)
                {
                    status = subNodesStatus;
                }
            }

            if (status < _internalStatus)
            {
                status = _internalStatus;
            }
            //foreach (var node in SubNodes)
            //{
            //    if (node.Status == TextConstants.Warning && status == TextConstants.Ok)
            //        status = TextConstants.Warning;
            //    if (node.Status == TextConstants.Error &&
            //        (status == TextConstants.Ok || status == TextConstants.Warning))
            //    {
            //        status = TextConstants.Error;
            //        //Status is error already
            //        break;
            //    }
                    
            //}

            //if (InternalStatus == TextConstants.Warning && status == TextConstants.Ok)
            //    status = TextConstants.Warning;
            //if (InternalStatus == TextConstants.Error && status != TextConstants.Error)
            //    status = TextConstants.Error;

            Status = status;
        }

        public string StatusDuration
        {
            get
            {
                if (_lastStatusUpdate.ToBinary() == 0)
                    _lastStatusUpdate = DateTime.Now;
                TimeSpan duration = DateTime.Now - _lastStatusUpdate;
                return duration.ToString(@"hh\:mm\:ss");
            }
        }
        private ObservableCollection<MonitoringNodeBase> _subNodes;

        public ObservableCollection<MonitoringNodeBase> SubNodes
        {
            get { return _subNodes; }
            set
            {
                foreach (var subNode in value)
                {
                    subNode._parent = this;
                }

                _subNodes = value;
                OnPropertyChanged(nameof(SubNodes));
            }
        }

        private ObservableCollection<MonitoringSensorViewModel> _sensors;

        public ObservableCollection<MonitoringSensorViewModel> Sensors
        {
            get { return _sensors; }
            set
            {
                foreach (var sensor in value)
                {
                    sensor.Parent = this;
                }

                _sensors = value;
                OnPropertyChanged(nameof(Sensors));
            }
        }

        public void Update(List<MonitoringSensorUpdate> sensorUpdates)
        {
            foreach (var sensorUpd in sensorUpdates)
            {
                if (!_nameToNode.ContainsKey(sensorUpd.Product))
                {
                    MonitoringNodeBase node = new MonitoringNodeBase(sensorUpd.Product, this);
                    _nameToNode[sensorUpd.Product] = node;
                    //SynchronizationContext.Current.Send(x => SubNodes.Add(node), null);
                    Application.Current.Dispatcher.Invoke( delegate { SubNodes.Add(node); });
                }
                //SynchronizationContext.Current.Send(x => _nameToNode[sensorUpd.Product].Update(sensorUpd, 0), null);
                _nameToNode[sensorUpd.Name].Update(sensorUpd, 0);
            }
        }
        //Pass the node number, 0 for the root
        public void Update(MonitoringSensorUpdate sensorUpdate, int level)
        {
            if (level < sensorUpdate.Path.Count - 1)
            {
                AddSubNode(sensorUpdate.Path[level]);
                _nameToNode[sensorUpdate.Path[level]].Update(sensorUpdate, level + 1);
                return;
            }

            if (level == sensorUpdate.Path.Count - 1)
            {
                UpdateSensor(sensorUpdate);
                return;
            }

            //TODO: throw an exception
        }

        public void AddSubNode(string subNodeName)
        {
            if (!_nameToNode.ContainsKey(subNodeName))
            {
                MonitoringNodeBase node = new MonitoringNodeBase(subNodeName, this);
                _nameToNode[subNodeName] = node;
                SubNodes.Add(node);
            }
        }

        public void UpdateSensor(MonitoringSensorUpdate sensorUpdate)
        {
            if (sensorUpdate.ActionType == ActionTypes.Remove)
            {
                _nameToSensor.Remove(sensorUpdate.Name);
                return;
            }

            if (!_nameToSensor.ContainsKey(sensorUpdate.Name))
            {
                MonitoringSensorViewModel sensor = new MonitoringSensorViewModel(sensorUpdate, this);
                _nameToSensor[sensorUpdate.Name] = sensor;
                Sensors.Add(sensor);
            }
            _nameToSensor[sensorUpdate.Name].Update(sensorUpdate);
        }
    }
}
