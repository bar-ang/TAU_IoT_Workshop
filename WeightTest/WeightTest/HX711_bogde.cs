using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
namespace WeightTest
{
    /**
     * I took the library from this link https://github.com/bogde/HX711
     * it was written in C++ for Arduino. I've adapted the code to our's needs.
     */
    class HX711_bogde
    {
        byte GAIN;      // amplification factor
        long OFFSET = 0;    // used for tare weight
        float SCALE = 1;    // used to return weight in grams, kg, ounces, whatever

        // define clock and data pin, channel, and gain factor
        // channel selection is made by passing the appropriate gain: 128 or 64 for channel A, 32 for channel B
        // gain: 128 or 64 for channel A; channel B works with 32 gain factor only

        //PD_SCK
        private GpioPin PowerDownAndSerialClockInput;

        //DOUT
        private GpioPin SerialDataOutput;

        private GpioController gpio;

        public HX711_bogde(byte dout, byte pd_sck, byte gain = 128)
        {
            gpio = GpioController.GetDefault();
            SerialDataOutput = gpio.OpenPin(dout);
            PowerDownAndSerialClockInput = gpio.OpenPin(pd_sck);

            PowerDownAndSerialClockInput.SetDriveMode(GpioPinDriveMode.Output);
            SerialDataOutput.SetDriveMode(GpioPinDriveMode.Input);

            set_gain(gain);
        }


        public HX711_bogde(GpioPin serialDataOutput, GpioPin powerDownAndSerialClockInput, byte gain = 128)
        {
            PowerDownAndSerialClockInput = powerDownAndSerialClockInput;
            powerDownAndSerialClockInput.SetDriveMode(GpioPinDriveMode.Output);

            SerialDataOutput = serialDataOutput;
            SerialDataOutput.SetDriveMode(GpioPinDriveMode.Input);

            set_gain(gain);
        }

        public void releaseGPIO()
        {
            PowerDownAndSerialClockInput.Dispose();
            SerialDataOutput.Dispose();
        }

        // check if HX711 is ready
        // from the datasheet: When output data is not ready for retrieval, digital output pin DOUT is high. Serial clock
        // input PD_SCK should be low. When DOUT goes to low, it indicates vodata is ready for retrieval.
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
                    GAIN = 1;
                    break;
                case 64:        // channel A, gain factor 64
                    GAIN = 3;
                    break;
                case 32:        // channel B, gain factor 32
                    GAIN = 2;
                    break;
            }

            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            read();
        }

        // waits for the chip to be ready and returns a reading
        public long read()
        {
            // wait for the chip to become ready
            while (!is_ready())
            {
                // Will do nothing on Arduino but prevent resets of ESP8266 (Watchdog Issue)
            }

            ulong value = 0;
            uint[] data = { 0, 0, 0 };
            uint filler = 0x00;

            // pulse the clock pin 24 times to read the data
            //TODO check if < 24 or <= 24
            for (int pulses = 0; pulses < 24; pulses++)
            {
                PowerDownAndSerialClockInput.Write(GpioPinValue.High);
                PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
                data[(int)(pulses/8)] += (uint)SerialDataOutput.Read();
            }
            //data[2] = shiftIn(DOUT, PD_SCK, MSBFIRST);
            //data[1] = shiftIn(DOUT, PD_SCK, MSBFIRST);
            //data[0] = shiftIn(DOUT, PD_SCK, MSBFIRST);

            // set the channel and the gain factor for the next reading using the clock pin
            for (uint i = 0; i < GAIN; i++)
            {
                SerialDataOutput.Write(GpioPinValue.High);
                SerialDataOutput.Write(GpioPinValue.Low);
            }

            // Replicate the most significant bit to pad out a 32-bit signed integer
            //TODO: maybe == 0?
            if ((data[2] & 0x80) != 0)
            {
                filler = 0xFF;
            }
            else
            {
                filler = 0x00;
            }

            // Construct a 32-bit signed integer
            value = ((ulong) (filler) << 24
                     | (ulong)(data[2]) << 16
                       | (ulong)(data[1]) << 8
                         | (ulong)(data[0]) );

            return (long)(value);
        }

        // returns an average reading; times = how many times to read
        public long read_average(byte times = 10)
        {
            long sum = 0;
            for (byte i = 0; i < times; i++)
            {
                sum += read();
            }
            return sum / times;
        }

        // returns (read_average() - OFFSET), that is the current value without the tare weight; times = how many readings to do
        public double get_value(byte times = 1)
        {
            return read_average(times) - OFFSET;
        }

        // returns get_value() divided by SCALE, that is the raw value divided by a value obtained via calibration
        // times = how many readings to do
        //TODO casting
        public float get_units(byte times = 1)
        {
            return (float)get_value(times) / SCALE;
        }

        // set the OFFSET value for tare weight; times = how many times to read the tare value
        //TODO casting
        public void tareAndSet(byte times = 10)
        {
            set_offset();
            double sum = read_average(times);
            set_offset((long)sum);
        }

        // set the OFFSET value for tare weight; times = how many times to read the tare value
        //TODO casting
        public long tare(byte times = 10)
        {
            set_offset();
            double sum = read_average(times);
            set_offset((long)sum);
            return (long)sum;
        }

        // set the SCALE value; this value is used to convert the raw data to "human readable" data (measure units)
        public void set_scale(float scale = 1.0f)
        {
            SCALE = scale;
        }

        // get the current SCALE
        public float get_scale()
        {
            return SCALE;
        }

        // set OFFSET, the value that's subtracted from the actual reading (tare weight)
        public void set_offset(long offset = 0)
        {
            OFFSET = offset;
        }

        // get the current OFFSET
        public long get_offset()
        {
            return OFFSET;
        }

        // puts the chip into power down mode
        public void power_down()
        {
            SerialDataOutput.Write(GpioPinValue.Low);
            SerialDataOutput.Write(GpioPinValue.High);
        }

        // wakes up the chip after power down mode
        public void power_up()
        {
            SerialDataOutput.Write(GpioPinValue.Low);
        }
    }
}
