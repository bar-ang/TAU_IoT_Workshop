using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RaspIO
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            IOMethod("sample.txt");   
        }
        public async void IOMethod(string filename)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
           // StorageFile sampleFile = await storageFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
           // await FileIO.WriteTextAsync(sampleFile, "Is this the real life?");

            StorageFile reader = await storageFolder.GetFileAsync(filename);
            string text = await FileIO.ReadTextAsync(reader);

            System.Diagnostics.Debug.WriteLine("write and the read: " + text);
        }
    }
}
