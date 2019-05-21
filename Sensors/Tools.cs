using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sensors
{
    class Tools
    {
        // Readonly values
        internal static readonly string APP_DIR = Tizen.Applications.Application.Current.DirectoryInfo.Data;

        // Common constants
        internal const string TAG = "EasyTrack";
        internal const ushort CHANNEL_ID = 104;
        internal const ushort SENSOR_SAMPLING_INTERVAL = 1000; // milliseconds
        internal const string HEALTHINFO_PRIVILEGE = "http://tizen.org/privilege/healthinfo";

        // Actions
        public const byte REQUEST_DATA = 0x01;

        // Sensors
        internal const ushort ACCELEROMETER = 1;
        internal const ushort GRAVITY = 2;
        internal const ushort GYROSCOPE = 3;
        internal const ushort HRM = 4;
        internal const ushort HUMIDITY = 5;
        internal const ushort LIGHT = 6;
        internal const ushort LINEARACCELERATION = 7;
        internal const ushort MAGNETOMETER = 8;
        internal const ushort ORIENTATION = 9;
        internal const ushort PRESSURE = 10;
        internal const ushort PROXIMITY = 11;
        internal const ushort TEMPERATURE = 12;
        internal const ushort ULTRAVIOLET = 13;
    }
}
