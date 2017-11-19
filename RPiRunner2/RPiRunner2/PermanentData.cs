using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;
using System.IO;
namespace RPiRunner2
{
    /// <summary>
    /// Manages the data that is saved on the SSD card (the long-term memory)
    /// </summary>
    class PermanentData
    {
        public const double BEST_OFFSET = -261614.7f;
        public const double BEST_SCALE = -13228.89f;

        public const string DEFAULT_FILE = "smart_weight_data.swd";
        public const string NULL_SYMBOL = "#";
        private static string devname;
        private static string serial;
        private static string lastseenIP;
        private static string currIP;
        private static double offset;
        private static double scale;
        private static string username;
        private static string password;

        public static string Devname { get => devname; set => devname = value; }
        public static string Serial { get => serial; set => serial = value; }
        public static string LastseenIP { get => lastseenIP; set => lastseenIP = value; }
        public static string CurrIP { get => currIP; set => currIP = value; }
        public static double Offset { get => offset; set => offset = value; }
        public static double Scale { get => scale; set => scale = value; }
        public static string Username { get => username; set => username = value; }
        public static string Password { get => password; set => password = value; }

        public static string auth()
        {
            return username + ":" + password;
        }

        public static async Task LoadFromMemoryAsync(string file)
        {
            PermData data;
            string dataRead = "default";
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile dataFile = await storageFolder.GetFileAsync(file);
                dataRead = await FileIO.ReadTextAsync(dataFile);
                data = JsonConvert.DeserializeObject<PermData>(dataRead);
            }
            catch(FileNotFoundException e)
            {
                System.Diagnostics.Debug.WriteLine("File not found: " + e.FileName);
                data = new PermData();
            }
            
            devname = data.devname;
            serial = data.serial;
            lastseenIP = data.lastseenIP;
            currIP = lastseenIP;
            scale = data.scale;
            offset = data.offset;
            username = data.username;
            password = data.password;

            System.Diagnostics.Debug.WriteLine("read from memory: " + dataRead);
        }
        public static async Task WriteToMemoryAsync(string file)
        {
            PermData pd = new PermData();
            pd.devname = devname;
            pd.serial = serial;
            pd.lastseenIP = currIP;
            pd.offset = offset;
            pd.scale = scale;
            pd.username = username;
            pd.password = password;
            string data = JsonConvert.SerializeObject(pd);

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile dataFile = await storageFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(dataFile, data);

            System.Diagnostics.Debug.WriteLine("written to memory: " + data);
        }

        public static async Task WriteToMemoryAsync()
        {
            await WriteToMemoryAsync(DEFAULT_FILE);
        }
        public static async Task LoadFromMemoryAsync()
        {
            await LoadFromMemoryAsync(DEFAULT_FILE);
        }

        private class PermData
        {
            public string devname = "AALSmartWeight";
            public string serial = NULL_SYMBOL;
            public string lastseenIP = NULL_SYMBOL; 
            public double offset = BEST_OFFSET;
            public double scale = BEST_SCALE;
            public string username = "admin";
            public string password = "admin";
        }
    }  
}
