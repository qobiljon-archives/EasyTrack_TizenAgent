﻿using System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using System.IO;
using System.Text;
using Tizen.Sensor;
using EasyTrackTizenAgent.Model;
using Tizen.Security;
using System.Threading;
using System.Collections.Generic;
using Samsung.Sap;
using System.Threading.Tasks;
using System.Net.Http;
using System.Json;
using Prefs = Tizen.Applications.Preference;
using Xamarin.Forms;

namespace EasyTrackTizenAgent
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : CirclePage
    {
        public MainPage()
        {
            InitializeComponent();

            dataRateMap = new Dictionary<int, int>();
            sensorMap = new Dictionary<int, Sensor>();
            initDataSourcesWithPrivileges();
            //new Thread(async () => await initAgentConnection()).Start();
        }

        #region Variables
        private Dictionary<int, int> dataRateMap;
        private Dictionary<int, Sensor> sensorMap;
        #endregion

        protected override void OnAppearing()
        {
            terminateFilesCounterThread();
            try
            {
                startFilesCounterThread();
            }
            catch (Exception e)
            {
                Toast.DisplayText(e.Message);
            }

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            terminateFilesCounterThread();

            base.OnDisappearing();
        }



        private void initDataSourcesWithPrivileges()
        {
            PrivacyPrivilegeManager.ResponseContext context = null;
            if (PrivacyPrivilegeManager.GetResponseContext(Tools.HEALTHINFO_PRIVILEGE).TryGetTarget(out context))
                context.ResponseFetched += (s, e) =>
                {
                    if (e.result != RequestResult.AllowForever)
                    {
                        Toast.DisplayText("Please provide the necessary privileges for the application to run!");
                        Environment.Exit(1);
                    }
                    else
                        initCampaignDataSources();
                };
            else
            {
                Toast.DisplayText("Please provide the necessary privileges for the application to run!");
                Environment.Exit(1);
            }

            switch (PrivacyPrivilegeManager.CheckPermission(Tools.HEALTHINFO_PRIVILEGE))
            {
                case CheckResult.Allow:
                    initCampaignDataSources();
                    break;
                case CheckResult.Deny:
                    Toast.DisplayText("Please provide the necessary privileges for the application to run!");
                    Environment.Exit(1);
                    break;
                case CheckResult.Ask:
                    PrivacyPrivilegeManager.RequestPermission(Tools.HEALTHINFO_PRIVILEGE);
                    break;
                default:
                    break;
            }
        }

        private void initCampaignDataSources()
        {
            #region Assign sensor model references
            accelerometerModel = new AccelerometerModel
            {
                IsSupported = Accelerometer.IsSupported,
                SensorCount = Accelerometer.Count
            };
            gravityModel = new GravityModel
            {
                IsSupported = GravitySensor.IsSupported,
                SensorCount = GravitySensor.Count
            };
            gyroscopeModel = new GyroscopeModel
            {
                IsSupported = Gyroscope.IsSupported,
                SensorCount = Gyroscope.Count
            };
            hRMModel = new HRMModel
            {
                IsSupported = HeartRateMonitor.IsSupported,
                SensorCount = HeartRateMonitor.Count
            };
            humidityModel = new HumidityModel
            {
                IsSupported = HumiditySensor.IsSupported,
                SensorCount = HumiditySensor.Count
            };
            lightModel = new LightModel
            {
                IsSupported = LightSensor.IsSupported,
                SensorCount = LightSensor.Count
            };
            linearAccelerationModel = new LinearAccelerationModel
            {
                IsSupported = LinearAccelerationSensor.IsSupported,
                SensorCount = LinearAccelerationSensor.Count
            };
            magnetometerModel = new MagnetometerModel
            {
                IsSupported = Magnetometer.IsSupported,
                SensorCount = Magnetometer.Count
            };
            orientationModel = new OrientationModel
            {
                IsSupported = OrientationSensor.IsSupported,
                SensorCount = OrientationSensor.Count
            };
            pressureModel = new PressureModel
            {
                IsSupported = PressureSensor.IsSupported,
                SensorCount = PressureSensor.Count
            };
            proximityModel = new ProximityModel
            {
                IsSupported = ProximitySensor.IsSupported,
                SensorCount = ProximitySensor.Count
            };
            temperatureModel = new TemperatureModel
            {
                IsSupported = TemperatureSensor.IsSupported,
                SensorCount = TemperatureSensor.Count
            };
            ultravioletModel = new UltravioletModel
            {
                IsSupported = UltravioletSensor.IsSupported,
                SensorCount = UltravioletSensor.Count
            };
            #endregion

            #region Assign sensor references and sensor measurement event handlers
            if (accelerometerModel.IsSupported)
            {
                accelerometer = new Accelerometer();
                accelerometer.PausePolicy = SensorPausePolicy.None;
                accelerometer.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                accelerometer.DataUpdated += storeAccelerometerDataCallback;
                sensorMap[Tools.ACCELEROMETER] = accelerometer;
            }
            if (gravityModel.IsSupported)
            {
                gravity = new GravitySensor();
                gravity.PausePolicy = SensorPausePolicy.None;
                gravity.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                gravity.DataUpdated += storeGravitySensorDataCallback;
                sensorMap[Tools.GRAVITY] = gravity;
            }
            if (gyroscopeModel.IsSupported)
            {
                gyroscope = new Gyroscope();
                gyroscope.PausePolicy = SensorPausePolicy.None;
                gyroscope.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                gyroscope.DataUpdated += storeGyroscopeDataCallback;
                sensorMap[Tools.GYROSCOPE] = gyroscope;
            }
            if (hRMModel.IsSupported)
            {
                hRM = new HeartRateMonitor();
                hRM.PausePolicy = SensorPausePolicy.None;
                hRM.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                hRM.DataUpdated += storeHeartRateMonitorDataCallback;
                sensorMap[Tools.HRM] = hRM;
            }
            if (humidityModel.IsSupported)
            {
                humidity = new HumiditySensor();
                humidity.PausePolicy = SensorPausePolicy.None;
                humidity.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                humidity.DataUpdated += storeHumiditySensorDataCallback;
                sensorMap[Tools.HUMIDITY] = humidity;
            }
            if (lightModel.IsSupported)
            {
                light = new LightSensor();
                light.PausePolicy = SensorPausePolicy.None;
                light.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                light.DataUpdated += storeLightSensorDataCallback;
                sensorMap[Tools.LIGHT] = light;
            }
            if (linearAccelerationModel.IsSupported)
            {
                linearAcceleration = new LinearAccelerationSensor();
                linearAcceleration.PausePolicy = SensorPausePolicy.None;
                linearAcceleration.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                linearAcceleration.DataUpdated += storeLinearAccelerationSensorDataCallback;
                sensorMap[Tools.LINEARACCELERATION] = linearAcceleration;
            }
            if (magnetometerModel.IsSupported)
            {
                magnetometer = new Magnetometer();
                magnetometer.PausePolicy = SensorPausePolicy.None;
                magnetometer.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                magnetometer.DataUpdated += storeMagnetometerDataCallback;
                sensorMap[Tools.MAGNETOMETER] = magnetometer;
            }
            if (orientationModel.IsSupported)
            {
                orientation = new OrientationSensor();
                orientation.PausePolicy = SensorPausePolicy.None;
                orientation.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                orientation.DataUpdated += storeOrientationSensorDataCallback;
                sensorMap[Tools.ORIENTATION] = orientation;
            }
            if (pressureModel.IsSupported)
            {
                pressure = new PressureSensor();
                pressure.PausePolicy = SensorPausePolicy.None;
                pressure.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                pressure.DataUpdated += storePressureSensorDataCallback;
                sensorMap[Tools.PRESSURE] = pressure;
            }
            if (proximityModel.IsSupported)
            {
                proximity = new ProximitySensor();
                proximity.PausePolicy = SensorPausePolicy.None;
                proximity.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                proximity.DataUpdated += storeProximitySensorDataCallback;
                sensorMap[Tools.PROXIMITY] = proximity;
            }
            if (temperatureModel.IsSupported)
            {
                temperature = new TemperatureSensor();
                temperature.PausePolicy = SensorPausePolicy.None;
                temperature.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                temperature.DataUpdated += storeTemperatureSensorDataCallback;
                sensorMap[Tools.TEMPERATURE] = temperature;
            }
            if (ultravioletModel.IsSupported)
            {
                ultraviolet = new UltravioletSensor();
                ultraviolet.PausePolicy = SensorPausePolicy.None;
                ultraviolet.Interval = Tools.DEFAULT_SENSOR_SAMPLING_INTERVAL;
                ultraviolet.DataUpdated += storeUltravioletSensorDataCallback;
                sensorMap[Tools.ULTRAVIOLET] = ultraviolet;
            }
            #endregion

            loadCampaignSettings();
        }

        private void loadCampaignSettings()
        {
            new Thread(async () =>
            {
                HttpResponseMessage response = await Tools.post(Tools.API_GET_CAMPAIGN_SETTINGS, new Dictionary<string, string>()
                    {
                        { "username", Prefs.Get<string>("username") },
                        { "password", Prefs.Get<string>("password") },
                        { "campaign_id", Prefs.Get<int>("campaign_id").ToString() },
                        { "device", "wearable-tizen" }
                    });
                if (response.IsSuccessStatusCode)
                {
                    JsonValue resJson = JsonValue.Parse(await response.Content.ReadAsStringAsync());
                    ServerResult resCode = (ServerResult)int.Parse(resJson["result"].ToString());
                    if (resCode == ServerResult.OK)
                    {
                        JsonObject campaignSettingsJson = (JsonObject)resJson["campaign_settings"];
                        foreach (JsonObject dataSourceJson in (JsonArray)campaignSettingsJson["data_sources"])
                        {
                            string device = dataSourceJson["device"];
                            int dataRate = dataSourceJson["data_rate"];
                            if (device.Equals("wearable-tizen"))
                            {
                                dataRateMap[dataSourceJson["source_id"]] = dataSourceJson["data_rate"];
                                sensorMap[dataSourceJson["source_id"]].Interval = Math.Max(dataSourceJson["data_rate"], Tools.MINIMUM_SENSOR_SAMPLING_INTERVAL);
                            }
                        }
                        log($"Campaign settings loaded! ({dataRateMap.Count} sources set up)");
                    }
                    else
                    {
                        Toast.DisplayText("Failed to load campaign settings! Please refer to the campaign creator!");
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Toast.DisplayText("Failed to load campaign settings! Please refer to the campaign creator!");
                    Environment.Exit(0);
                }
            }).Start();
        }

        private async Task initAgentConnection()
        {
            try
            {
                agent = await Agent.GetAgent(
                    profile: "/kr/ac/inha/nsl/easytrack",
                    onConnect: (con) =>
                    {
                        try
                        {
                            if (con.Peer.ProfileVersion == agent.ProfileVersion)
                            {
                                con.DataReceived += (sender, evt) =>
                                {
                                    if (evt.Channel.ID == Tools.CHANNEL_ID)
                                    {
                                        byte request = evt.Data[0];

                                        if (request == Tools.REQUEST_DATA)
                                            reportDataCollection(reportDataColButton, new EventArgs());
                                    }
                                };
                                conn = con;
                                peer = conn.Peer;
                                Device.BeginInvokeOnMainThread(new Action(() => Toast.DisplayText("Connected to Android Agent!")));
                                return true;
                            }
                            else
                            {
                                Device.BeginInvokeOnMainThread(new Action(() => Toast.DisplayText("Couldn't connect to Android Agent!")));
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            Device.BeginInvokeOnMainThread(new Action(() => Toast.DisplayText($"Couldn't connect to Android Agent! Message: {e.Message}")));
                            return false;
                        }
                    }
                );
            }
            catch (Exception e)
            {
                Device.BeginInvokeOnMainThread(new Action(() => Toast.DisplayText(e.Message)));
            }
        }

        #region Variables
        // Log properties
        private Thread filesCounterThread;
        private Thread submitDataThread;
        private string openLogStreamStamp;
        private StreamWriter logStreamWriter;
        private int logLinesCount = 1;
        private int filesCount;
        private bool stopFilesCounterThread;
        private bool stopSubmitDataThread;

        // Connection properties
        private Agent agent;
        private Connection conn;
        private Peer peer;

        // Sensors and their SensorModels
        internal Accelerometer accelerometer { get; private set; }
        internal AccelerometerModel accelerometerModel { get; private set; }
        internal GravityModel gravityModel { get; private set; }
        internal GravitySensor gravity { get; private set; }
        internal GyroscopeModel gyroscopeModel { get; private set; }
        internal Gyroscope gyroscope { get; private set; }
        internal HRMModel hRMModel { get; private set; }
        internal HeartRateMonitor hRM { get; private set; }
        internal HumidityModel humidityModel { get; private set; }
        internal HumiditySensor humidity { get; private set; }
        internal LightModel lightModel { get; private set; }
        internal LightSensor light { get; private set; }
        internal LinearAccelerationModel linearAccelerationModel { get; private set; }
        internal LinearAccelerationSensor linearAcceleration { get; private set; }
        internal MagnetometerModel magnetometerModel { get; private set; }
        internal Magnetometer magnetometer { get; private set; }
        internal OrientationModel orientationModel { get; private set; }
        internal OrientationSensor orientation { get; private set; }
        internal PressureModel pressureModel { get; private set; }
        internal PressureSensor pressure { get; private set; }
        internal ProximityModel proximityModel { get; private set; }
        internal ProximitySensor proximity { get; private set; }
        internal TemperatureModel temperatureModel { get; private set; }
        internal TemperatureSensor temperature { get; private set; }
        internal UltravioletModel ultravioletModel { get; private set; }
        internal UltravioletSensor ultraviolet { get; private set; }
        #endregion

        #region UI Event callbacks
        private void reportDataCollection(object sender, EventArgs e)
        {
            if (submitDataThread != null && submitDataThread.IsAlive)
                terminateSubmitDataThread();
            else
                startSubmitDataThread();
        }
        private void startDataCollectionClick(object sender, EventArgs e)
        {
            log("Sensor data collection started");
            Tizen.System.Power.RequestCpuLock(0);

            if (dataRateMap.ContainsKey(Tools.ACCELEROMETER))
                accelerometer?.Start();
            if (dataRateMap.ContainsKey(Tools.GRAVITY))
                gravity?.Start();
            if (dataRateMap.ContainsKey(Tools.GYROSCOPE))
                gyroscope?.Start();
            if (dataRateMap.ContainsKey(Tools.HRM))
                hRM?.Start();
            if (dataRateMap.ContainsKey(Tools.HUMIDITY))
                humidity?.Start();
            if (dataRateMap.ContainsKey(Tools.LIGHT))
                light?.Start();
            if (dataRateMap.ContainsKey(Tools.LINEARACCELERATION))
                linearAcceleration?.Start();
            if (dataRateMap.ContainsKey(Tools.MAGNETOMETER))
                magnetometer?.Start();
            if (dataRateMap.ContainsKey(Tools.ORIENTATION))
                orientation?.Start();
            if (dataRateMap.ContainsKey(Tools.PRESSURE))
                pressure?.Start();
            if (dataRateMap.ContainsKey(Tools.PROXIMITY))
                proximity?.Start();
            if (dataRateMap.ContainsKey(Tools.TEMPERATURE))
                temperature?.Start();
            if (dataRateMap.ContainsKey(Tools.ULTRAVIOLET))
                ultraviolet?.Start();

            startDataColButton.IsEnabled = false;
            stopDataColButton.IsEnabled = true;
        }
        private void stopDataCollectionClick(object sender, EventArgs e)
        {
            log("Sensor data collection stopped");
            Tizen.System.Power.ReleaseCpuLock();

            if (dataRateMap.ContainsKey(Tools.ACCELEROMETER))
                accelerometer?.Stop();
            if (dataRateMap.ContainsKey(Tools.GRAVITY))
                gravity?.Stop();
            if (dataRateMap.ContainsKey(Tools.GYROSCOPE))
                gyroscope?.Stop();
            if (dataRateMap.ContainsKey(Tools.HRM))
                hRM?.Stop();
            if (dataRateMap.ContainsKey(Tools.HUMIDITY))
                humidity?.Stop();
            if (dataRateMap.ContainsKey(Tools.LIGHT))
                light?.Stop();
            if (dataRateMap.ContainsKey(Tools.LINEARACCELERATION))
                linearAcceleration?.Stop();
            if (dataRateMap.ContainsKey(Tools.MAGNETOMETER))
                magnetometer?.Stop();
            if (dataRateMap.ContainsKey(Tools.ORIENTATION))
                orientation?.Stop();
            if (dataRateMap.ContainsKey(Tools.PRESSURE))
                pressure?.Stop();
            if (dataRateMap.ContainsKey(Tools.PROXIMITY))
                proximity?.Stop();
            if (dataRateMap.ContainsKey(Tools.TEMPERATURE))
                temperature?.Stop();
            if (dataRateMap.ContainsKey(Tools.ULTRAVIOLET))
                ultraviolet?.Stop();

            startDataColButton.IsEnabled = true;
            stopDataColButton.IsEnabled = false;
        }
        #endregion

        #region Sensor DataUpdated Callbacks
        private void storeAccelerometerDataCallback(object sender, AccelerometerDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter?.Flush();
            logStreamWriter?.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.ACCELEROMETER},Accelerometer, -1,{e.X},{e.Y},{e.Z}");
        }
        private void storeGravitySensorDataCallback(object sender, GravitySensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.GRAVITY},GravitySensor,-1,{e.X},{e.Y},{e.Z}");
            }
        }
        private void storeGyroscopeDataCallback(object sender, GyroscopeDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.GYROSCOPE},Gyroscope,-1,{e.X},{e.Y},{e.Z}");
            }
        }
        private void storeHeartRateMonitorDataCallback(object sender, HeartRateMonitorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.HRM},HeartRateMonitor,-1,{e.HeartRate}");
            }
        }
        private void storeHumiditySensorDataCallback(object sender, HumiditySensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.HUMIDITY},HumiditySensor,-1,{e.Humidity}");
            }
        }
        private void storeLightSensorDataCallback(object sender, LightSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.LIGHT},LightSensor,-1,{e.Level}");
            }
        }
        private void storeLinearAccelerationSensorDataCallback(object sender, LinearAccelerationSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.LINEARACCELERATION},LinearAccelerationSensor,-1,{e.X},{e.Y},{e.Z}");
            }
        }
        private void storeMagnetometerDataCallback(object sender, MagnetometerDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.MAGNETOMETER},Magnetometer,-1,{e.X},{e.Y},{e.Z}");
            }
        }
        private void storeOrientationSensorDataCallback(object sender, OrientationSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.ORIENTATION},OrientationSensor,-1,{e.Azimuth}, {e.Pitch}, {e.Roll}");
            }
        }
        private void storePressureSensorDataCallback(object sender, PressureSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.PRESSURE},PressureSensor,-1,{e.Pressure}");
            }
        }
        private void storeProximitySensorDataCallback(object sender, ProximitySensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.PROXIMITY},ProximitySensor,-1,{e.Proximity}");
            }
        }
        private void storeTemperatureSensorDataCallback(object sender, TemperatureSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.TEMPERATURE},TemperatureSensor,-1,{e.Temperature}");
            }
        }
        private void storeUltravioletSensorDataCallback(object sender, UltravioletSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"wearable-tizen,{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()},{Tools.ULTRAVIOLET},UltravioletSensor,-1,{e.UltravioletIndex}");
            }
        }
        #endregion

        private void reportToETAgent(
            string message = default(string),
            string path = default(string),
            EventHandler<FileTransferFinishedEventArgs> fileTransferFinishedHandler = null)
        {
            if (message != default(string))
            {
                conn.Send(agent.Channels[Tools.CHANNEL_ID], Encoding.UTF8.GetBytes(message));
                Debug.WriteLine(Tools.TAG, $"Message has been submitted on BLE. length={message.Length}");
            }
            else if (path != default(string))
            {
                OutgoingFileTransfer oft = new OutgoingFileTransfer(peer, path);
                oft.Send();
                if (fileTransferFinishedHandler == null)
                    oft.Finished += (s, e) => { Debug.WriteLine(Tools.TAG, $"File has been submitted on BLE. Result => {e.Result}"); };
                else
                    oft.Finished += fileTransferFinishedHandler;
            }
            log("Data uploaded");
        }

        private async Task reportToApiServer(
            string message = default(string),
            string path = default(string),
            Task postTransferTask = null)
        {
            if (message != default(string))
            {
                HttpResponseMessage result = await Tools.post(Tools.API_NOTIFY, new Dictionary<string, string> {
                    { "username", Tizen.Applications.Preference.Get<string>("username") },
                    { "password", Tizen.Applications.Preference.Get<string>("password") },
                    { "message", message }
                });
                if (result.IsSuccessStatusCode)
                {
                    JsonValue resJson = JsonValue.Parse(await result.Content.ReadAsStringAsync());
                    log($"RESULT: {resJson["result"]}");
                    Debug.WriteLine(Tools.TAG, $"Message has been submitted to the Server. length={message.Length}");
                }
                else
                    Toast.DisplayText("Failed to submit a notification to server!");
            }
            else if (path != null)
            {
                HttpResponseMessage result = await Tools.post(
                    Tools.API_SUBMIT_DATA,
                    new Dictionary<string, string>
                    {
                        { "username", Tizen.Applications.Preference.Get<string>("username") },
                        { "password", Tizen.Applications.Preference.Get<string>("password") }
                    },
                    File.ReadAllBytes(path)
                );
                if (result == null)
                {
                    Toast.DisplayText("Please check your WiFi connection first!");
                    return;
                }
                if (result.IsSuccessStatusCode)
                {
                    JsonValue resJson = JsonValue.Parse(await result.Content.ReadAsStringAsync());
                    ServerResult resCode = (ServerResult)int.Parse(resJson["result"].ToString());
                    if (resCode == ServerResult.OK)
                        postTransferTask?.Start();
                    else
                        log($"Failed to upload {path.Substring(path.LastIndexOf(Path.PathSeparator) + 1)}");
                }
                else
                    log($"Failed to upload {path.Substring(path.LastIndexOf(Path.PathSeparator) + 1)}");
            }
        }

        internal void log(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (logLinesCount == logLabel.MaxLines)
                    logLabel.Text = $"{logLabel.Text.Substring(logLabel.Text.IndexOf('\n') + 1)}\n{message}";
                else
                {
                    logLabel.Text = $"{logLabel.Text}\n{message}";
                    logLinesCount++;
                }
            });
        }

        private void eraseSensorData()
        {
            foreach (string file in Directory.GetFiles(Tools.APP_DIR, "*.csv"))
                File.Delete(file);
        }

        private int countSensorDataFiles()
        {
            return Directory.GetFiles(Tools.APP_DIR, "*.csv").Length;
        }

        private void checkUpdateCurrentLogStream()
        {
            DateTime nowTimestamp = DateTime.Now;
            nowTimestamp = new DateTime(year: nowTimestamp.Year, month: nowTimestamp.Month, day: nowTimestamp.Day, hour: nowTimestamp.Hour, minute: nowTimestamp.Minute, second: 0);
            string nowStamp = $"{new DateTimeOffset(nowTimestamp).ToUnixTimeMilliseconds()}";

            if (logStreamWriter == null)
            {
                openLogStreamStamp = nowStamp;
                string filePath = Path.Combine(Tools.APP_DIR, $"{nowStamp}.csv");
                logStreamWriter = new StreamWriter(path: filePath, append: true);

                log("Data-log file created/attached");
                Tools.sendHeartBeatMessage();
            }
            else if (!nowStamp.Equals(openLogStreamStamp))
            {
                logStreamWriter.Flush();
                logStreamWriter.Close();

                openLogStreamStamp = nowStamp;
                string filePath = Path.Combine(Tools.APP_DIR, $"{nowStamp}.csv");
                logStreamWriter = new StreamWriter(path: filePath, append: false);

                log("New data-log file created");
                Tools.sendHeartBeatMessage();
            }
        }

        private void terminateFilesCounterThread()
        {
            stopFilesCounterThread = true;
            filesCounterThread?.Join();
            stopFilesCounterThread = false;
        }

        private void startFilesCounterThread()
        {
            filesCounterThread = new Thread(() =>
            {
                using (FileSystemWatcher watcher = new FileSystemWatcher(Tools.APP_DIR))
                {
                    filesCount = countSensorDataFiles();

                    Device.BeginInvokeOnMainThread(() => { filesCountLabel.Text = $"FILES: {filesCount}"; });

                    watcher.Filter = "*.csv";
                    watcher.Deleted += (s, e) => { Device.BeginInvokeOnMainThread(() => { filesCountLabel.Text = $"FILES: {--filesCount}"; }); };
                    watcher.Created += (s, e) => { Device.BeginInvokeOnMainThread(() => { filesCountLabel.Text = $"FILES: {++filesCount}"; }); };

                    watcher.EnableRaisingEvents = true;
                    while (!stopFilesCounterThread)
                        Thread.Sleep(100);

                    watcher.EnableRaisingEvents = false;
                }
            });
            filesCounterThread.IsBackground = true;
            filesCounterThread.Start();
        }

        private void terminateSubmitDataThread()
        {
            stopSubmitDataThread = true;
            submitDataThread?.Join();
            stopSubmitDataThread = false;
        }

        private void startSubmitDataThread()
        {
            try
            {
                submitDataThread = new Thread(async () =>
                {
                    try
                    {
                        // Get list of files and sort in increasing order
                        string[] filePaths = Directory.GetFiles(Tools.APP_DIR, "*.csv");
                        List<long> fileNamesInLong = new List<long>();
                        for (int n = 0; !stopSubmitDataThread && n < filePaths.Length; n++)
                        {
                            string tmp = filePaths[n].Substring(filePaths[n].LastIndexOf('/') + 1);
                            fileNamesInLong.Add(long.Parse(tmp.Substring(0, tmp.LastIndexOf('.'))));
                        }
                        fileNamesInLong.Sort();

                        // Submit files to server except the last file
                        terminateFilesCounterThread();
                        for (int n = 0; !stopSubmitDataThread && n < fileNamesInLong.Count - 1; n++)
                        {
                            Device.BeginInvokeOnMainThread(() => { filesCountLabel.Text = $"{(n + 1) * 100 / fileNamesInLong.Count}% UPLOADED"; });
                            string filepath = Path.Combine(Tools.APP_DIR, $"{fileNamesInLong[n]}.csv");
                            await reportToApiServer(path: filepath, postTransferTask: new Task(() => { File.Delete(filepath); }));
                        }
                        Device.BeginInvokeOnMainThread(() => { filesCountLabel.Text = $"100% UPLOADED"; });
                        Thread.Sleep(300);
                        startFilesCounterThread();
                    }
                    catch (Exception e)
                    {
                        Device.BeginInvokeOnMainThread(new Action(() => Toast.DisplayText(e.Message)));
                    }
                });
                submitDataThread.IsBackground = true;
                submitDataThread.Start();
            }
            catch (Exception e)
            {
                Toast.DisplayText(e.Message);
            }
        }
    }
}
