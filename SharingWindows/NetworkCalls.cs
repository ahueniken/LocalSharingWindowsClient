using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using SharingWindows.Properties;
using NativeWifi;

public static class NetworkCalls
{

    public static void Login(string pwd)
    {
        if (checkCorrectWifi()) { 
            String email = (string)Settings.Default["Username"]; 
            String password = pwd; 

            CookieContainer cookies = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://local-experiences.herokuapp.com/users/sign_in");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Apache-HttpClient/UNAVAILABLE (java 1.4)";
            request.ServicePoint.Expect100Continue = false;
            request.CookieContainer = cookies;

            request.AllowAutoRedirect = false;

            string postData = "user[email]=" + email + "&user[password]=" + password + "&commit=Log in";
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = bytes.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode == HttpStatusCode.Found)
            {
                response.Close();

                String location = response.Headers[HttpResponseHeader.Location];

                request = WebRequest.Create(location) as HttpWebRequest;
                request.CookieContainer = cookies;
                request.Method = WebRequestMethods.Http.Get;

                HttpWebResponse response2 = request.GetResponse() as HttpWebResponse;

                Settings.Default["auth_token"] = response2.Headers["Auth_token"];
                if (response2.Headers["Auth_token"] != null) { 
                Settings.Default["shouldBroadcast"] = true;
                } else {
                    Settings.Default["shouldBroadcast"] = false;
                }

                Console.WriteLine("Auth token is: " + response2.Headers["Auth_token"]);
                // Release resources of response object.
                response2.Close(); 
            }
        }
    }

    public static void ShareApp(string title, string description)
    {
        String email = (string)Settings.Default["Username"]; 

        string auth_token = (string) Settings.Default["auth_token"];
        bool shouldBroadcast = (bool)Settings.Default["shouldBroadcast"];

        if (auth_token != null && shouldBroadcast && checkCorrectWifi()) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://local-experiences.herokuapp.com/actions/share");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string postData = "title=" + title + "&description=" + description + "&user_token=" + auth_token + "&user_email=" + email;
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = bytes.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            var result = reader.ReadToEnd();
            //Console.WriteLine(result);
            stream.Dispose();
            reader.Dispose();
        }
    }

    private static bool checkCorrectWifi()
    {
        var wlanClient = new WlanClient();

        foreach (WlanClient.WlanInterface wlanInterface in wlanClient.Interfaces)
        {
            Wlan.WlanConnectionAttributes current = wlanInterface.CurrentConnection;

            byte[] conMacAddr = current.wlanAssociationAttributes.dot11Bssid;
            var conMacAddrLen = (uint)conMacAddr.Length;
            var constr = new string[(int)conMacAddrLen];
            for (int i = 0; i < conMacAddrLen; i++)
            {
                constr[i] = conMacAddr[i].ToString("x2");
            }
            string conMac = string.Join("", constr);
            Console.WriteLine(conMac);

            return conMac.Equals("000fb556fde0");
        }
        return false;
    }
}
