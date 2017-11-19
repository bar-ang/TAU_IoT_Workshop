using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using ZXing;

namespace weighJune28
{
    [Activity(Label = "QRScan")]
    public class QRScan : Activity
    {
        ImageView _imageView;
        TextView tv;
        string qrcode = "No QR found";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.QRView);
            Button StartWeight = FindViewById<Button>(Resource.Id.StartWeigh);
            //StartWeight.Visibility = ViewStates.Gone;
            //button.Click += delegate {
            //    var activity2 = new Intent(this, typeof(Activity2));
            //    activity2.PutExtra("MyData", "Data from Activity1");
            //    StartActivity(activity2);
            //};
            StartWeight.Click += delegate {
                var getIPAdd = new Intent(this, typeof(GetIPAddress));
                getIPAdd.PutExtra("qrcode", qrcode);
                StartActivity(getIPAdd);
            };


            Button button = FindViewById<Button>(Resource.Id.btn_takepic);
            _imageView = FindViewById<ImageView>(Resource.Id.img_showpic);
            tv = FindViewById<TextView>(Resource.Id.tv_result);
            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();
                button.Click += TakeAPicture;
            }
        }

        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "CameraAppDemo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }
        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }
        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));
            StartActivityForResult(intent, 0);
        }
        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display
            // Loading the full sized image will consume to much memory 
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = _imageView.Height;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
            if (App.bitmap != null)
            {
                _imageView.SetImageBitmap(App.bitmap);

                scanBarcode(App.bitmap, tv);

                App.bitmap = null;
            }

            // Dispose of the Java side bitmap.
            GC.Collect();
        }
        private void scanBarcode(Bitmap bitmap, TextView tv)
        {
            // create a barcode reader instance
            var barcodeReader = new BarcodeReader();

            barcodeReader.AutoRotate = true;
            barcodeReader.Options.TryHarder = true;

            // decode the barcode from the in memory bitmap
            ZXing.Result barcodeResult = barcodeReader.Decode(bitmap);

            if (barcodeResult != null)
            {
                tv.Text = barcodeResult.Text;
                qrcode = barcodeResult.Text;
                FindViewById<Button>(Resource.Id.StartWeigh).Visibility = ViewStates.Visible;
                
            }

            else
            {
                tv.Text = "No QR found. Please try to scan again.";
                FindViewById<Button>(Resource.Id.btn_takepic).Text = "Open Camera to scan code again";
                //DELETE LATER !!!!
                //qrcode = "Testing 2";
                //FindViewById<Button>(Resource.Id.StartWeigh).Visibility = ViewStates.Visible;
            }
                
        }
    }

    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }
}
