﻿namespace Sensors.Model
{
    public class UltravioletModel : BaseSensorModel
    {
        private float ultravioletIndex;

        public float UltravioletIndex
        {
            get { return ultravioletIndex; }
            set
            {
                ultravioletIndex = value;
                OnPropertyChanged();
            }
        }
    }
}