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
using Windows.Devices.Gpio;
using System.Diagnostics;
using System.Threading.Tasks;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WeightTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const byte DOUT_PIN = 26;
        const byte SLK_PIN = 19;
        private const int AVG_NUM = 1000;

        private GpioPin dout;
        private GpioPin slk;
        private GpioController gpio;
        private LinearHX hx711b;

        public const float BEST_OFFSET = -261614.7f;
        public const float BEST_SCALE = -13228.89f;

        public MainPage()
        {
            this.InitializeComponent();

            gpio = GpioController.GetDefault();
            slk = gpio.OpenPin(SLK_PIN);
            dout = gpio.OpenPin(DOUT_PIN);
            hx711b = new LinearHX(dout, slk, 128);


            hx711b.power_down();
            Debug.WriteLine("down");
            hx711b.power_up();
            Debug.WriteLine("up");

            hx711b.setParameters(BEST_OFFSET, BEST_SCALE);
            txt_offset.Text = BEST_OFFSET.ToString();
            txt_scale.Text = BEST_SCALE.ToString();
        }

        bool inMidCalibration = false;
        float rawNull;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*
             * How to Calibrate Your Scale:
             * 1. Call set_scale() with no parameter.
             * 2. Call tare() with no parameter.
             * 3. Place a known weight on the scale and call get_units(10).
             * 4. Divide the result in step 3 to your known weight. You should get about the parameter you need to pass to set_scale.
             * 5. Adjust the parameter in step 4 until you get an accurate reading.
            */
           

           

            
            if (!inMidCalibration)
            {
                txt_results.Text = "";
                appendLine("==Calibration==", txt_results);
                rawNull = hx711b.getRawWeight(AVG_NUM);
                inMidCalibration = true;
                appendLine("Now set an object with a known weight, and press 'calibrate' again.", txt_results);
                //txt_offset.Text = offset.ToString();
            }
            else
            {
                float knownWeight = float.Parse(txt_knownWeight.Text);
                float rawWeight = hx711b.getRawWeight(AVG_NUM);
                hx711b.setParameters(rawNull, rawWeight, knownWeight);

                appendLine("", txt_results);
                appendLine("Your weight scale is", txt_results);
                appendLine(hx711b.Scale.ToString(), txt_results);
                appendLine("Your weight offset is", txt_results);
                appendLine(hx711b.Offset.ToString(), txt_results);
                txt_scale.Text = hx711b.Scale.ToString();
                txt_offset.Text = hx711b.Offset.ToString();
                inMidCalibration = false;
                appendLine("==Done Calibrating==", txt_results);
                appendLine("==Testing==", txt_results);
                appendLine("You currenty weighing (should be " + knownWeight.ToString() + "):", txt_results);
                float rw = hx711b.getWeight(AVG_NUM);
                appendLine(hx711b.transform(rw).ToString(), txt_results);
                appendLine("Raw data:", txt_results);
                appendLine(rw.ToString(), txt_results);
            }
        }

        private void writeText(string text, TextBox textbox)
        {
            textbox.Text = text;
        }


    
        private void appendLine(string text, TextBox tb)
        {
            tb.Text += text + "\n";
            //Debug.WriteLine(text);
        }
        private void Button_weigh2_Click(object sender, RoutedEventArgs e)
        {
            txt_results.Text = "";

            appendLine("==Weighting Results==", txt_results);

            //hx711b.set_scale(float.Parse(txt_scale.Text));
            //hx711b.set_offset(long.Parse(txt_offset.Text));
            //hx711b.tare();


            // print the average of AVG_NUM readings from the ADC minus the tare weight (not set yet)
            appendLine("*Data after scaling:", txt_results);
            float res = hx711b.getWeight(AVG_NUM) ;
            appendLine(res.ToString(), txt_results);
            appendLine(" ", txt_results);

            appendLine("SCALE:", txt_results);
            
            appendLine(hx711b.Scale.ToString(), txt_results);
            appendLine("OFFSET:", txt_results);
            appendLine(hx711b.Offset.ToString(), txt_results);
            appendLine(" ", txt_results);

            Debug.WriteLine("w: " + res + " s: " + hx711b.Scale + " o: " + hx711b.Offset);

        }
        private async void Clock(int avgOn)
        {
            float raw;
            Queue<float> lastMes = new Queue<float>();
            float avg = 0;
            int c;
            Debug.WriteLine("tick");
            float t = 0;
            while (true)
            {
                raw = t; // hx711b.getRawWeight(1);
                t++;

                if (lastMes.Count == 0)
                    avg = raw;
                else if (lastMes.Count == avgOn)
                {
                    c = lastMes.Count;
                    avg += (raw - lastMes.Dequeue()) / (float)c;
                }
                else
                {
                    c = lastMes.Count;
                    avg += (avg * c + raw) / (float)(c + 1);
                }
                lastMes.Enqueue(raw);
                // txt_clock_raw.Text = avg.ToString();
                // txt_clock.Text = hx711b.transform(avg).ToString();
                Debug.WriteLine("raw: " + avg + ", weight: " + hx711b.transform(avg));
                await Task.Delay(1000);
            }
        }
        private void btn_tare_Click(object sender, RoutedEventArgs e)
        {
            float rawNull;
            txt_results.Text = "";
            appendLine("==Tare==", txt_results);
            rawNull = hx711b.getRawWeight(AVG_NUM);
            hx711b.Offset = rawNull;
            txt_offset.Text = rawNull.ToString();
            appendLine("Tared. New offset is:", txt_results);
            appendLine(rawNull.ToString(), txt_results);
        }

        private void btn_accuracy_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => readrawAsync(hx711b, txt_results));
        }

        private void readrawAsync(LinearHX hx711b, TextBox tb)
        {
            uint r;
            while (true)
            {
                r = hx711b.read();
            }
        }

        private void Button_test_Click(object sender, RoutedEventArgs e)
        {
            int[] meas;
            float standart, smart;
            while (true)
            {
                meas = hx711b.getListOfMeasures(AVG_NUM);
                standart = hx711b.transform(hx711b.avg(meas));
                hx711b.detectAnomalies(meas, (int)(AVG_NUM * 0.1), 0.95f);
                smart =hx711b.transform( hx711b.avg(meas));

                Debug.WriteLine("Without filtering: " + standart + "\tWith Filtering: " + smart);
            }

        }

        private void Button_set_Click(object sender, RoutedEventArgs e)
        {
            float scale = float.Parse(txt_scale.Text);
            float offset = float.Parse(txt_offset.Text);
            hx711b.setParameters(offset, scale);
            appendLine("offset: " + hx711b.Offset + ",\t scale: " + scale, txt_results);
        }
        /*private void Button_weigh2_Click(object sender, RoutedEventArgs e)
{
txt_results.Text = "";

appendLine("==Weighting Results==", txt_results);

hx711b.set_scale();
//hx711b.tare();

// print a raw reading from the ADC
appendLine("Raw Data:", txt_results);
appendLine(hx711b.read().ToString(), txt_results);
appendLine(" ", txt_results);

// print the average of 20 readings from the ADC
appendLine("Avg of 20 readings:", txt_results);
appendLine(hx711b.read_average(AVG_NUM).ToString(), txt_results);
appendLine(" ", txt_results);

// print the average of 5 readings from the ADC minus the tare weight (not set yet)
appendLine("Data after scaling:", txt_results);
appendLine(hx711b.get_units(AVG_NUM).ToString(), txt_results);
appendLine(" ", txt_results);

hx711b.set_scale(float.Parse(txt_scale.Text));
//hx711b.tare();

appendLine("==Calibrated==", txt_results);
appendLine("Calculating data again.", txt_results);

// print a raw reading from the ADC
appendLine("*Raw Data:", txt_results);
appendLine(hx711b.read().ToString(), txt_results);
appendLine(" ", txt_results);

// print the average of 20 readings from the ADC
appendLine("*Avg of 100 readings:", txt_results);
appendLine(hx711b.read_average(AVG_NUM).ToString(), txt_results);
appendLine(" ", txt_results);

// print the average of 20 readings from the ADC minus the tare weight (not set yet)
appendLine("*Data after scaling:", txt_results);
appendLine(hx711b.get_units(AVG_NUM).ToString(), txt_results);
appendLine(" ", txt_results);

}*/


    }
    /*
   private void Button_Click_1Async(object sender, RoutedEventArgs e)
   {

       Button_Click_1(sender, e);
   }

   private void Button_Click_1(object sender, RoutedEventArgs e)
   {
       gpio = GpioController.GetDefault();
       dout = gpio.OpenPin(DOUT_PIN);
       slk = gpio.OpenPin(SLK_PIN);

       //dout.Write(GpioPinValue.High);
       //dout.Write(GpioPinValue.High);
       //while (true)
       //{

       //}

       hx711 = new HX711(slk, dout);

       float g;
       hx711.PowerOn();
       txt_results.Text = "";
       for (int i = 0; i < 25; i++)
       {
           g = hx711.GetGram() - 16842.14f;
           Debug.WriteLine(g);
           txt_results.Text += g.ToString() + "\n";
       }
       hx711.PowerDown();
       Debug.WriteLine("===");
   }
   */

}
