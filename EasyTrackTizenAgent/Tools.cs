using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTrackTizenAgent
{
    class Tools
    {
        // Readonly values
        internal static readonly string APP_DIR = Tizen.Applications.Application.Current.DirectoryInfo.Data;

        // Common constants
        internal const string TAG = "EasyTrack";
        internal const ushort CHANNEL_ID = 104;
        internal const uint DEFAULT_SENSOR_SAMPLING_INTERVAL = 1000; // milliseconds
        internal const uint MINIMUM_SENSOR_SAMPLING_INTERVAL = 250; // milliseconds
        internal const string HEALTHINFO_PRIVILEGE = "http://tizen.org/privilege/healthinfo";
        // API constants
        internal const string API_REGISTER = "register";
        internal const string API_UNREGISTER = "unregister";
        internal const string API_AUTHENTICATE = "authenticate";
        internal const string API_SUBMIT_HEARTBEAT = "heartbeat";
        internal const string API_SUBMIT_DATA = "submit_data";
        internal const string API_NOTIFY = "notify";
        internal const string API_GET_CAMPAIGN_SETTINGS = "get_campaign_settings";


        // Actions
        public const byte REQUEST_DATA = 0x01;

        // Sensors
        internal const ushort ACCELEROMETER = 0x01;
        internal const ushort GRAVITY = 0x02;
        internal const ushort GYROSCOPE = 0x03;
        internal const ushort HRM = 0x04;
        internal const ushort HUMIDITY = 0x05;
        internal const ushort LIGHT = 0x06;
        internal const ushort LINEARACCELERATION = 0x07;
        internal const ushort MAGNETOMETER = 0x08;
        internal const ushort ORIENTATION = 0x09;
        internal const ushort PRESSURE = 0x0a;
        internal const ushort PROXIMITY = 0x0b;
        internal const ushort TEMPERATURE = 0x0c;
        internal const ushort ULTRAVIOLET = 0x0d;
        internal static readonly ushort[] ALL_SENSORS = { ACCELEROMETER, GRAVITY, GYROSCOPE, HRM, HUMIDITY, LIGHT, LINEARACCELERATION, MAGNETOMETER, ORIENTATION, PRESSURE, PROXIMITY, TEMPERATURE, ULTRAVIOLET };

        internal async static Task<HttpResponseMessage> post(string api, Dictionary<string, string> body, byte[] fileContent = null)
        {
            const string SERVER_URL = "http://165.246.43.97:36012";

            if (fileContent == null)
                using (HttpClient client = new HttpClient())
                    return await client.PostAsync($"{SERVER_URL}/{api}", new FormUrlEncodedContent(body));
            else
                using (HttpContent bytesContent = new ByteArrayContent(fileContent))
                using (MultipartFormDataContent formData = new MultipartFormDataContent())
                using (HttpClient client = new HttpClient())
                {
                    foreach (var elem in body)
                        formData.Add(new StringContent(elem.Value), elem.Key, elem.Key);
                    formData.Add(bytesContent, "file", "file");
                    return await client.PostAsync($"{SERVER_URL}/{api}", formData);
                }
        }

        internal static void sendHeartBeatMessage()
        {
            new Thread(async () =>
            {
                await post(API_SUBMIT_HEARTBEAT, new Dictionary<string, string>
                {
                    { "username", Tizen.Applications.Preference.Get<string>("username") },
                    { "password", Tizen.Applications.Preference.Get<string>("password") }
                });
            }).Start();
        }
    }

    // Result codes from server
    enum ServerResult
    {
        OK = 0,
        FAIL = 1,
        BAD_JSON_PARAMETERS = 2,
        USERNAME_TAKEN = 3,
        TOO_SHORT_PASSWORD = 4,
        TOO_LONG_PASSWORD = 5,
        USER_DOES_NOT_EXIST = 6,
        BAD_PASSWORD = 7
    }
}
