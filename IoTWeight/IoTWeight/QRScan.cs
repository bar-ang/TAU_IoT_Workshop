using Android.App;
using Android.Widget;
using Android.Content;
using Android.OS;
using ZXing.Mobile;

namespace IoTWeight
{
    [Activity(Label = "Please Scan the Barcode")]
    public class QRScan : Activity
    {
        //The component should work on Android 2.2 or higher.
        Button buttonScan;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.QRView);
            FindViewById<TextView>(Resource.Id.QRresult).Text = "";

            MobileBarcodeScanner.Initialize(Application);

            buttonScan = FindViewById<Button>(Resource.Id.button1);

            buttonScan.Click += ButtonScan_Click;

            
        }

        private async void ButtonScan_Click(object sender, System.EventArgs e)
        {
            var scanner = new MobileBarcodeScanner();
            scanner.UseCustomOverlay = false;
            var result = await scanner.Scan(MobileBarcodeScanningOptions.Default);

            TextView QRtextView = FindViewById<TextView>(Resource.Id.QRresult);
            if (result != null)
            {
                System.Diagnostics.Debug.WriteLine("Scanned Barcode: " + result.Text);
                Toast.MakeText(ApplicationContext, "Scanned Barcode: " + result.Text, ToastLength.Long).Show();
                QRtextView.Text = "";

                string qrcode = result.Text;
                var getIPAdd = new Intent(this, typeof(GetIPAddress));
                getIPAdd.PutExtra("qrcode", qrcode);
                StartActivity(getIPAdd);
               
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No QR Found :-(");
                //Toast.MakeText(ApplicationContext, "No QR Found :-(", ToastLength.Long).Show();
                QRtextView.Text = "No QR Found. Please Scan again";
            }
        }
    }
}

