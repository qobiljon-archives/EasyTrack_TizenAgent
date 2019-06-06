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
        internal const uint SENSOR_SAMPLING_INTERVAL = 1000; // milliseconds
        internal const string HEALTHINFO_PRIVILEGE = "http://tizen.org/privilege/healthinfo";
        // API constants
        internal const string API_REGISTER = "register";
        internal const string API_UNREGISTER = "unregister";
        internal const string API_AUTHENTICATE = "authenticate";
        internal const string API_SUBMIT_HEARTBEAT = "heartbeat";
        internal const string API_SUBMIT_DATA = "submit_data";
        internal const string API_NOTIFY = "notify";


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

        internal async static Task<HttpResponseMessage> post(string api, Dictionary<string, string> body, byte[] fileContent = null)
        {
            const string SERVER_URL = "http://165.246.43.162:36012";
            // const string SERVER_URL = "http://165.246.43.163:9876";

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
