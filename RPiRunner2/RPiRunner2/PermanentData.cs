﻿using System;
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
        public const float BEST_OFFSET = -261614.7f;
        public const float BEST_SCALE = -13228.89f;

        public const string DEFAULT_FILE = "smart_weight_data.swd";
        public const string NULL_SYMBOL = "#";
        private static string devname;
        private static string serial;
        private static string lastseenIP;
        private static string currIP;
        private static float offset;
        private static float scale;

        public static string Devname { get => devname; set => devname = value; }
        public static string Serial { get => serial; set => serial = value; }
        public static string LastseenIP { get => lastseenIP; set => lastseenIP = value; }
        public static string CurrIP { get => currIP; set => currIP = value; }
        public static float Offset { get => offset; set => offset = value; }
        public static float Scale { get => scale; set => scale = value; }

        public static async void LoadFromMemory(string file)
        {
            PermData data;
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile dataFile = await storageFolder.GetFileAsync(file);
                string dataRead = await FileIO.ReadTextAsync(dataFile);
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

            System.Diagnostics.Debug.WriteLine("read from memory: " + data);
        }
        public static async void WriteToMemory(string file)
        {
            PermData pd = new PermData();
            pd.devname = devname;
            pd.serial = serial;
            pd.lastseenIP = currIP;
            pd.offset = offset;
            pd.scale = scale;
            string data = JsonConvert.SerializeObject(pd);

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile dataFile = await storageFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(dataFile, data);

            System.Diagnostics.Debug.WriteLine("written to memory: " + data);
        }

        public static void WriteToMemory()
        {
            WriteToMemory(DEFAULT_FILE);
        }
        public static void LoadFromMemory()
        {
            LoadFromMemory(DEFAULT_FILE);
        }

        private class PermData
        {
            public string devname = "AALSmartWeight";
            public string serial = NULL_SYMBOL;
            public string lastseenIP = NULL_SYMBOL; 
            public float offset = BEST_OFFSET;
            public float scale = BEST_SCALE;
        }
    }  
}