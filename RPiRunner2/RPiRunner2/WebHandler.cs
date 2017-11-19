using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Networking.Connectivity;
using System.Text.RegularExpressions;
using System.Net;

namespace RPiRunner2
{
    /// <summary>
    /// Managing the System's web server that is used for the installation
    /// </summary>
    class WebHandler
    {
        const string applicationURL = @"https://iotweight.azurewebsites.net";

        public const int WEB_PORT = 9000;
        public const int MAX_PASS_LENGTH = 12;
        public const int MIN_PASS_LENGTH = 4;

        public const string PAGE = "SettingsPage.html";

        private HTTPServer http;
        private UserHardwareLinker uhl;

        public WebHandler(UserHardwareLinker uhl)
        {
            //Initializing web server
            this.http = new HTTPServer(WEB_PORT);
            this.uhl = uhl;
            http.OnDataRecived += http_OnDataRecived;
            http.OnError += http_OnError;
            Task httpTask = http.Start();
            httpTask.Wait();
            System.Diagnostics.Debug.WriteLine("web server created");
        }

        /// <summary>
        /// parse the HTTP request that was received from the user to get the data that he/she has sent.
        /// </summary>
        /// <param name="data">the HTTP request query</param>
        /// <returns>
        /// A dictionary of the data received.
        /// e.g. if the user tries to login by sending the username and the password, then this method will return a dictionary where:
        /// "username" -> "myuser"
        /// "password" -> "Aa123456"
        /// </returns>
        public Dictionary<string, string> getQuery(string data)
        {
            Dictionary<string, string> queryFields = new Dictionary<string, string>();

            string[] fields;
            string query;
            if (data.Split(' ').Length > 1)
            {
                query = data.Split(' ')[1];
                if (query.IndexOf("/?") == 0)
                {
                    query = query.Substring(2);
                    System.Diagnostics.Debug.WriteLine(query);
                    fields = query.Split('&');
                    foreach (string field in fields)
                    {
                        string prop, val;
                        string[] sep = field.Split('=');
                        prop = sep[0];
                        val = sep[1];
                        queryFields.Add(sep[0], sep[1]);//TODO do something with the data
                    }
                }
            }
            return queryFields;
        }

        /// <summary>
        /// Receives an HTTP request and check if it contains login details and if these details are correct
        /// </summary>
        /// <param name="headers">HTTP request splitted to rows</param>
        /// <returns>True if login details are exist and correct</returns>
        public bool authenticate(string[] headers)
        {
            bool auth = false;
            string[] temp;
            string basic;
            foreach (string header in headers)
            {
                if (header.ToLower().IndexOf("authorization") >= 0)
                {
                    temp = header.Split(' ');
                    basic = temp[temp.Length - 1].Trim();

                    byte[] decbasic = Convert.FromBase64String(basic);
                    string decodedString = Encoding.UTF8.GetString(decbasic);

                    if (decodedString.Equals(PermanentData.auth()))
                    {
                        auth = true;
                    }
                }
            }
            return auth;
        }

        //calibration received data
        double nullweight;
        double knownWeight;
        double rawWeight;

        /// <summary>
        /// This method is activated every time a message has received from the user.
        /// </summary>
        /// <param name="data">The first row of the HTTP request query.</param>
        /// <param name="sender">the HTTPServer object that handles the current connection.</param>
        public async Task http_OnDataRecived(string data, Windows.Storage.Streams.DataWriter writer)
        {
            string[] headers = data.Split('\n');

            //Parse the message to extract the data from the request
            Dictionary<string, string> fields = getQuery(headers[0]);

            //Check if the request contains a login header with a valid user and password. If not, send '401 Unauthorized' response to the client.
            if (!authenticate(headers))
            {
                System.Diagnostics.Debug.WriteLine("@@Unauthorized");
                await http.Send(CreateHTTP.Code401_Unauthorized(), writer);
                return;
            }

            //get page
            string html = await HTTPServer.getHTMLAsync(PAGE);


            //fill the textboxes if the current data
            html = HTTPServer.HTMLInputFill(html, "text", "name", PermanentData.Devname);
            html = HTTPServer.HTMLInputFill(html, "text", "serial", PermanentData.Serial);
            html = HTTPServer.HTMLInputFill(html, "text", "man_offset", PermanentData.Offset.ToString());
            html = HTTPServer.HTMLInputFill(html, "text", "man_scale", PermanentData.Scale.ToString());

            //some browsers might ask for the page's icon.
            if (data.IndexOf("favicon.ico") >= 0)
            {
                await http.Send(CreateHTTP.Code204_NoContent(), writer);
                return;
            }

            double hardwtask = -100;
            try
            {
                /*determine which operation the client requested and do it.*/

                if (fields.Keys.Contains("chname"))
                {
                    //change name
                    System.Diagnostics.Debug.WriteLine("got: " + fields["name"]);
                    PermanentData.Devname = WebUtility.UrlDecode(fields["name"]);
                    html = HTTPServer.HTMLRewrite(html, "span", "name_feedback", "Your device's name was changed to  " + PermanentData.Devname);
                }
                if (fields.Keys.Contains("chpass"))
                {
                    //change password
                    string currpass = WebUtility.UrlDecode(fields["curr_pass"]);
                    string newpass = WebUtility.UrlDecode(fields["password"]);
                    string confirm = WebUtility.UrlDecode(fields["confirm"]);

                    if (!newpass.Equals(confirm))
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "Passwords do not match");
                    }
                    else if (!currpass.Equals(PermanentData.Password))
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "Password incorrect");
                    }
                    else if (HTTPServer.passwordValidation(newpass, MIN_PASS_LENGTH, MAX_PASS_LENGTH) < 0)
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "Password invalid<br/>The password should contain only letters and numbers and underscore ('_')<br/>Password must be 4-12 digits long.");
                    }
                    else
                    {
                        PermanentData.Password = newpass;
                        html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "Your password has changed successfully");
                    }
                }
                if (fields.Keys.Contains("register"))
                {
                    //set serial number
                    string serial = WebUtility.UrlDecode(fields["serial"]);
                    if (HTTPServer.passwordValidation(serial) < 0)
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "serial_feedback", "Serial invalid<br/>The Serial should contain only letters and numbers and underscore ('_')");
                    }
                    else
                    {
                        string ip = GetLocalIp();
                        await putRecordInDatabase(ip, serial);
                        PermanentData.Serial = serial;
                        PermanentData.CurrIP = ip;
                        html = HTTPServer.HTMLRewrite(html, "span", "serial_feedback", "Your device's serial is now  " + PermanentData.Serial);
                    }
                }

                if (fields.Keys.Contains("soffset") || fields.Keys.Contains("sscale"))
                {
                    //weighing something for calibration
                    if (uhl != null)
                    {
                        hardwtask = uhl.getRawWeight((int)(UserHardwareLinker.WEIGH_AVG * UserHardwareLinker.CALIB_FACTOR));
                        if (fields.Keys.Contains("soffset"))
                        {
                            nullweight = hardwtask;
                            System.Diagnostics.Debug.WriteLine("nullweight: " + nullweight);
                            html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "The sensor returned value: " + nullweight);
                        }
                        if (fields.Keys.Contains("sscale"))
                        {
                            rawWeight = hardwtask;
                            System.Diagnostics.Debug.WriteLine("rawWeight: " + rawWeight);
                            html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "The sensor returned value: " + rawWeight);
                        }
                    }
                    else
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Error: Your device does not have the sufficient hardware requerments.<br/>Operation did not complete.");
                    }
                }
                if (fields.Keys.Contains("calibrate"))
                {
                    //calibrate
                    try
                    {
                        if (uhl != null)
                        {
                            knownWeight = float.Parse(fields["known"]);
                            if (uhl.Scale != 0)
                            {
                                uhl.setParameters(nullweight, rawWeight, knownWeight);
                                html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "OK! the device's parameters are:<br />OFFSET: " + uhl.Offset + "<br />SCALE: " + uhl.Scale);
                                PermanentData.Scale = uhl.Scale;
                                PermanentData.Offset = uhl.Offset;
                            }
                            else
                            {
                                html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "the SCALE cannot be zero.");
                            }
                        }
                        else
                        {
                            html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Error: Your device does not have the sufficient hardware requerments.<br/>Operation did not complete.");
                        }
                    }
                    catch
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Operation failed.");
                    }
                }
                if (fields.Keys.Contains("man_calib"))
                {
                    //manual calibration
                    if (uhl != null)
                    {
                        float offset = float.Parse(fields["man_offset"]);
                        float scale = float.Parse(fields["man_scale"]);
                        if (scale != 0)
                        {
                            uhl.setParameters(offset, scale);
                            html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "OK! the device's parameters are:<br />OFFSET: " + uhl.Offset + "<br />SCALE: " + uhl.Scale);
                            PermanentData.Scale = uhl.Scale;
                            PermanentData.Offset = uhl.Offset;
                        }
                        else
                        {
                            html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "the SCALE cannot be zero.");
                        }
                    }
                    else
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Error: Your device does not have the sufficient hardware requerments.<br/>Operation did not complete.");
                    }
                }
                if (fields.Keys.Contains("best_calib"))
                {
                    //calibration using default values
                    if (uhl != null)
                    {
                        double offset = PermanentData.BEST_OFFSET;
                        double scale = PermanentData.BEST_SCALE;
                        uhl.setParameters(offset, scale);
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "OK! the device's parameters are:<br />OFFSET: " + uhl.Offset + "<br />SCALE: " + uhl.Scale);
                        PermanentData.Scale = uhl.Scale;
                        PermanentData.Offset = uhl.Offset;
                    }
                    else
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Error: Your device does not have the sufficient hardware requerments.<br/>Operation did not complete.");
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            html = HTTPServer.HTMLInputFill(html, "text", "name", PermanentData.Devname);
            html = HTTPServer.HTMLInputFill(html, "text", "serial", PermanentData.Serial);
            html = HTTPServer.HTMLInputFill(html, "text", "man_offset", PermanentData.Offset.ToString());
            html = HTTPServer.HTMLInputFill(html, "text", "man_scale", PermanentData.Scale.ToString());

            string response = CreateHTTP.Code200_Ok(html);

            Task writeTask = PermanentData.WriteToMemoryAsync();

            Task sendTask = http.Send(response, writer);

            await writeTask;
            await sendTask;

            System.Diagnostics.Debug.WriteLine("HTTP: page sent back to user");
        }

        public void http_OnError(string message)
        {
            System.Diagnostics.Debug.WriteLine("Internal Server Error: " + message);
        }

        /* end of installtion methods */
        /*  ============================================================================ */

        //Returns the device's IP Address
        private string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn =>
                            hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId && hn.CanonicalName.Split('.').Length == 4);
            // the ip address
            return hostname?.CanonicalName;
        }

        //Ron
        //This function inserts to the SQL database a record with the Raspberry's Ip address and QR code.
        //Also: In case the Raspberry's Ip address was changed,  this function updates the database with the current IP.
        public async Task putRecordInDatabase(string ipAdd, string QR_Code)
        {
            var client = new MobileServiceClient(applicationURL);
            //TODO: there is not async version od GetTable, but we need to make it async somwhow
            IMobileServiceTable<RaspberryTable> raspberryTableRef = client.GetTable<RaspberryTable>();
            try
            {
                List<RaspberryTable> ipAddressList = await raspberryTableRef.Where(item => (item.QRCode == QR_Code)).ToListAsync();
                if (ipAddressList.Count == 0)
                {
                    var record1 = new RaspberryTable
                    {
                        QRCode = QR_Code,
                        IPAddress = ipAdd,
                    };
                    await raspberryTableRef.InsertAsync(record1);
                }
                else    //case where we want to update an old ip address
                {
                    var address = ipAddressList[0];
                    address.IPAddress = ipAdd;
                    await raspberryTableRef.UpdateAsync(address);
                }


                //System.Diagnostics.Debug.WriteLine("i'm after insert");
            }
            catch (Exception e)
            {
                //TODO Ron
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}
