using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics;
using Accord.Math;
namespace RPiRunner2
{
    /**
    * Most of the code is taken from this link https://github.com/bogde/HX711
    * it was written in C++ for Arduino. I've adapted the code to our's needs.
    */
    class LinearHX
    {
        private double offset;
        private double scale;
        private int gain;

        //PD_SCK
        private GpioPin PowerDownAndSerialClockInput;

        //DOUT
        private GpioPin SerialDataOutput;

        private GpioController gpio;

        public double Offset { get => offset; set => offset = value; }
        public double Scale { get => scale; set => scale = value; }
        public int Gain { get => gain; set => gain = value; }
        public GpioPin PowerDownAndSerialClockInput1 { get => PowerDownAndSerialClockInput; set => PowerDownAndSerialClockInput = value; }
        public GpioPin SerialDataOutput1 { get => SerialDataOutput; set => SerialDataOutput = value; }
        public GpioController Gpio { get => gpio; set => gpio = value; }

        public LinearHX(GpioPin serialDataOutput, GpioPin powerDownAndSerialClockInput, byte gain = 128)
        {
            PowerDownAndSerialClockInput = powerDownAndSerialClockInput;
            powerDownAndSerialClockInput.SetDriveMode(GpioPinDriveMode.Output);

            SerialDataOutput = serialDataOutput;
            SerialDataOutput.SetDriveMode(GpioPinDriveMode.Input);

            offset = 0;
            scale = 1;

            set_gain(gain);
        }

        /*
         * Set the parameters of 'scale' and 'offset' for calibrating the scale
         * @param rawNull the value received when weighing nothing
         * @param rawWeight the value received when calculation an object with a known weight
         * @param realWeight the real known weight of the weighted object
         */
        public void setParameters(double rawNull, double rawWeight, double realWeight)
        {
            this.offset = rawNull;
            this.scale = (rawWeight - rawNull) / realWeight;
        }
        public void setParameters(double offset, double scale)
        {
            this.offset = offset;
            this.scale = scale;
        }

        /*
         * Gets the received data from the scale and transform it into the real weight of the object
         * You can manually specify the values of 'scale' and 'offset' or use the precalculated ones (which you have set by 'setParameters(..)')
         */
        public double transform(double rawData)
        {
            return transform(rawData, scale, offset);
        }
        public double transform(double rawData, double scale, double offset)
        {
            return (rawData - offset) / scale;
        }


        //(from bogde)
        // waits for the chip to be ready and returns a reading
        public uint read()
        {
            // wait for the chip to become ready
            while (!is_ready())
            {
                // Will do nothing on Arduino but prevent resets of ESP8266 (Watchdog Issue)
            }

            uint data = 0;

            // pulse the clock pin 24 times to read the data
            for (int pulses = 0; pulses < 24; pulses++)
            {
                PowerDownAndSerialClockInput.Write(GpioPinValue.High);
                PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
                data <<= 1;
                data |= (uint)SerialDataOutput.Read();
               // System.Diagnostics.Debug.Write((data & 0x1).ToString() + ",");
               
            }
            
            //smear the last bit
            if (((data>>0x17)&0x1) != 0)
            {
                data |= 0xff000000;
            }

            // set the channel and the gain factor for the next reading using the clock pin
            for (uint i = 0; i < gain; i++)
            {
                SerialDataOutput.Write(GpioPinValue.High);
                SerialDataOutput.Write(GpioPinValue.Low);
            }
            //if ((int)data != -1 && (int)data != -8388608)
               // System.Diagnostics.Debug.WriteLine("data(bits): " + data.ToString("X") + ",\t(" + ((int)data).ToString() + ")");
            return (uint)data;
        }

        public bool is_ready()
        {
            return SerialDataOutput.Read() == GpioPinValue.Low;
        }

        // set the gain factor; takes effect only after a call to read()
        // channel A can be set for a 128 or 64 gain; channel B has a fixed 32 gain
        // depending on the parameter, the channel is also set to either A or B
        public void set_gain(byte gain = 128)
        {
            switch (gain)
            {
                case 128:       // channel A, gain factor 128
                    this.gain = 1;
                    break;
                case 64:        // channel A, gain factor 64
                    this.gain = 3;
                    break;
                case 32:        // channel B, gain factor 32
                    this.gain = 2;
                    break;
            }

            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            read();
        }

        // returns an average reading; times = how many times to read
        // NOTE: reaturns raw data!
        public double getRawWeight(int times = 100)
        {
            long sum = 0;
            for (int i = 0; i < times; i++)
            {
                sum += (int)read();
            }
            return (double)sum / times;
        }

        //Using 'Anomaly Detection' method to filer out inaccurate measures
        //Note:measurment time may be longer
        public double smartGetRawWeight(int times = 100, double tolerance = 0.95f, double trainingGroupRatio = 0.1f)
        {
            int[] results = new int[times];
            long sum = 0;
            for (int i = 0; i < times; i++)
            {
                results[i] = (int)read();
            }

            detectAnomalies(results, (int)(times * trainingGroupRatio), tolerance);

            int count = 0;
            for (int i = 0; i < times; i++)
            {
                if (results[i] > int.MinValue)
                {
                    sum += results[i];
                    count++;
                }
            }
            return (double)sum / count;
        }

        public static void detectAnomalies(int[] results, int training, double tolerance)
        {
            int low;
            low = (results.Length - training) / 2;
            int[] sample = new int[training];
            for (int i = 0; i < training; i++)
                sample[i] = results[low + i];


            double mean;
            double std;


            mean = Measures.Mean(sample);
            std = Measures.StandardDeviation(sample, mean);

            if (std > 0)
            {

                NormalDistribution norm = new NormalDistribution(mean, std);

                double zScore;
                for (int i = 0; i < results.Length; i++)
                {
                    zScore = Math.Abs(results[i] - mean) / std;
                    if (2 * (1 - norm.DistributionFunction(zScore)) < (1 - tolerance))
                        results[i] = int.MinValue;
                }
            }
            else
            {
                for (int i = 0; i < results.Length; i++)
                {

                    if (results[i] != mean)
                        results[i] = int.MinValue;
                }
            }
        }

        public double getWeight(int times = 100)
        {
            double raw = getRawWeight(times);
            return transform(raw);
        }
        public double getWeight(double rawWeight)
        {
            return transform(rawWeight);
        }


        // puts the chip into power down mode
        public void power_down()
        {
            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            PowerDownAndSerialClockInput.Write(GpioPinValue.High);
        }

        // wakes up the chip after power down mode
        public void power_up()
        {
            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
        }
    }
}
