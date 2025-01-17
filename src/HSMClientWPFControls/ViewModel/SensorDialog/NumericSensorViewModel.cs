﻿using System.Collections.ObjectModel;
using HSMClientWPFControls.Model.SensorDialog;
using OxyPlot;
using OxyPlot.Series;

namespace HSMClientWPFControls.ViewModel.SensorDialog
{
    public class NumericSensorViewModel : DialogViewModel
    {
        public NumericSensorViewModel(ISensorDialogModel model) : base(model)
        {
        }

        public Collection<DataPoint> Data
        {
            get
            {
                var model = Model as INumericTimeValueModel;
                return model?.Data;
            }
            set
            {
                var model = Model as INumericTimeValueModel;
                if (model != null)
                {
                    model.Data = value;
                }
                OnPropertyChanged(nameof(Data));
            }
        }

        public string Count
        {
            get
            {
                var model = Model as INumericTimeValueModel;
                return model?.Count.ToString();
            }
            set
            {
                var model = Model as INumericTimeValueModel;
                if (model != null)
                    model.Count = int.Parse(value);

                OnPropertyChanged();
                OnPropertyChanged(nameof(Count));
            }
        }
    }
}
