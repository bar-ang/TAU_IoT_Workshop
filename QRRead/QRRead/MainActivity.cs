using Android.App;
using Android.Widget;
using Android.OS;
using ZXing.Mobile;

namespace QRRead
{
    [Activity(Label = "QRRead", MainLauncher = true)]
    public class MainActivity : Activity
    {
        //The component should work on Android 2.2 or higher.
        Button buttonScan;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            MobileBarcodeScanner.Initialize(Application);

            buttonScan = FindViewById<Button>(Resource.Id.button1);

            buttonScan.Click += ButtonScan_Click;

            System.Diagnostics.Debug.WriteLine("Try to say hello");
            Toast.MakeText(ApplicationContext, "HELLO!", ToastLength.Long).Show();
        }

        private async void ButtonScan_Click(object sender, System.EventArgs e)
        {
            var scanner = new MobileBarcodeScanner();
            scanner.UseCustomOverlay = false;
            var result = await scanner.Scan(MobileBarcodeScanningOptions.Default);




            if (result != null)
            {
                System.Diagnostics.Debug.WriteLine("Scanned Barcode: " + result.Text);
                Toast.MakeText(ApplicationContext, "Scanned Barcode: " + result.Text, ToastLength.Long).Show();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No QR Found :-(");
                Toast.MakeText(ApplicationContext, "No QR Found :-(", ToastLength.Long).Show();
            }
        }
    }
}

