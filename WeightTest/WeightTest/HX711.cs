using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace WeightTest
{
    class HX711
    {
        //PD_SCK
        private GpioPin PowerDownAndSerialClockInput;

        //DOUT
        private GpioPin SerialDataOutput;

        private long offset = 0;
        private float scale = SCALE_DEF;

        private const byte AVERAGE_DEF = 25;
        private const float SCALE_DEF = 1992.0f;

        public HX711(GpioPin powerDownAndSerialClockInput, GpioPin serialDataOutput)
        {
            PowerDownAndSerialClockInput = powerDownAndSerialClockInput;
            powerDownAndSerialClockInput.SetDriveMode(GpioPinDriveMode.Output);

            SerialDataOutput = serialDataOutput;
            SerialDataOutput.SetDriveMode(GpioPinDriveMode.Input);
        }

        public void calibrate(float knownWeight)
        {
            /*
             * How to Calibrate Your Scale:
             * Call set_scale() with no parameter.
             * Call tare() with no parameter.
             * Place a known weight on the scale and call get_units(10).
             * Divide the result in step 3 to your known weight. You should get about the parameter you need to pass to set_scale.
             * Adjust the parameter in step 4 until you get an accurate reading.
             */
            this.PowerOn();
            this.SetOffset(ReadAverage(AVERAGE_DEF));
            for (int i = 0; i < 1; i++)
            {
                float g = GetGram();
                this.SetScale(knownWeight / g);
                System.Diagnostics.Debug.WriteLine(g);
            }
            this.PowerDown();
        }

        public float PoweOnAndGetGram()
        {
            PowerOn();
            float g = GetGram();
            PowerDown();
            return g;
        }

        //When output data is not ready for retrieval,
        //digital output pin DOUT is high.
        private bool IsReady()
        {
            return SerialDataOutput.Read() == GpioPinValue.Low;
        }
        //By applying 25~27 positive clock pulses at the
        //PD_SCK pin, data is shifted out from the DOUT
        //output pin.Each PD_SCK pulse shifts out one bit,
        //starting with the MSB bit first, until all 24 bits are
        //shifted out.
        public int Read()
        {
            while (!IsReady())
            {

            }
            string binaryData = "";
            for (int pulses = 0; pulses < 25 + (int)InputAndGainSelection; pulses++)
            {
                PowerDownAndSerialClockInput.Write(GpioPinValue.High);
                PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
                if (pulses < 25)
                {
                    binaryData += (int)SerialDataOutput.Read();
                }
            }
            return Convert.ToInt32(binaryData, 2);
        }

        public long ReadAverage(byte times)
        {
            long sum = 0;
            for (byte i = 0; i < times; i++)
            {
                sum += Read();
            }
            return sum / times;
        }

        public long ReadAverage()
        {
            return ReadAverage(AVERAGE_DEF);
        }

        //When PD_SCK pin changes from low to high
        //and stays at high for longer than 60µs, HX711
        //enters power down mode
        public void PowerDown()
        {
            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            PowerDownAndSerialClockInput.Write(GpioPinValue.High);
            //wait 60 microseconds
        }

        //When PD_SCK returns to low,
        //chip will reset and enter normal operation mode
        public void PowerOn()
        {
            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            _InputAndGainSelection = InputAndGainOption.A128;
        }
        //After a reset or power-down event, input
        //selection is default to Channel A with a gain of 128. 

        private InputAndGainOption _InputAndGainSelection = InputAndGainOption.A128;

        public InputAndGainOption InputAndGainSelection
        {
            get
            {
                return _InputAndGainSelection;
            }
            set
            {
                _InputAndGainSelection = value;
                //Read();
            }
        }

        public void SetOffset(long offset)
        {
            this.offset = offset;
        }
        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public float GetGram()
        {
            long val = (ReadAverage() - this.offset);
            return (float)val / this.scale;
        }
    }
    public enum InputAndGainOption : int
    {
        A128 = 1, B32 = 2, A64 = 3
    }
}



