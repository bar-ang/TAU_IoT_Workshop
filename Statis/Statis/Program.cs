using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics;
using Accord.Math;
namespace Statis
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] meas = new int[] { -88, 4, 10, 4, 200, 3, 4, 4, 3, 5, 1, 100, 2, 3, 4, 4, 3, 2, 3, 4, 4, 3, 4, 3, 4, 300, 1 };
            System.Console.WriteLine("all results:\t" + avg(meas));
            detectAnomalies(meas, (int)(meas.Length * 0.2), 0.95f);
            System.Console.WriteLine("Filtered results:\t" + avg(meas));

            Console.ReadLine();
        }

        public static float avg(int[] res)
        {
            float mean = 0;
            int count = 0;
            for(int i = 0; i < res.Length; i++)
            {
                if(res[i] > int.MinValue)
                {
                    mean += res[i];
                    count++;
                }
            }
            return mean / count;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~`


        public static void detectAnomalies(int[] results, int training, float tolerance)
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
                    zScore = Math.Abs(results[i] - mean)/std;
                    if (2*(1-norm.DistributionFunction(zScore)) < (1 - tolerance))
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
    }
}
