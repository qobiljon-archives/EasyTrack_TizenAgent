using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using System.Threading;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prefs = Tizen.Applications.Preference;

namespace EasyTrackTizenAgent
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthenticationPage : CirclePage
    {
        public AuthenticationPage()
        {
            InitializeComponent();

            if (Prefs.Contains("logged_in") && Prefs.Get<bool>("logged_in"))
            {
                usernameEntry.Text = Prefs.Get<string>("username");
                passwordEntry.Text = Prefs.Get<string>("password");
            }
        }

        public void SignInClicked(object sender, EventArgs e)
        {
            IsEnabled = false;
            new Thread(async () =>
            {
                HttpResponseMessage response = await Tools.post(Tools.API_AUTHENTICATE, new Dictionary<string, string>
                {
                    { "username", usernameEntry.Text },
                    { "password", passwordEntry.Text }
                });
                if (response.IsSuccessStatusCode)
                {
                    JsonValue resJson = JsonValue.Parse(await response.Content.ReadAsStringAsync());
                    if ((ServerResult)(int)resJson["result"] == ServerResult.OK)
                    {
                        Prefs.Set("logged_in", true);
                        Prefs.Set("username", usernameEntry.Text);
                        Prefs.Set("password", passwordEntry.Text);
                        Prefs.Set("recruited", (bool)resJson["recruited"]);
                        Prefs.Set("campaign_id", (int)resJson["campaign_id"]);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            IsEnabled = true;
                            if (Prefs.Get<bool>("recruited"))
                            {
                                Navigation.PushModalAsync(new MainPage());
                            }
                            else
                                Toast.DisplayText($"You haven't registered for any campaign yet, please register first!");
                        });
                    }
                    else
                    {
                        Toast.DisplayText("Failed to log in, please try again!");
                        IsEnabled = true;
                    }
                }
            }).Start();
        }
    }
}
