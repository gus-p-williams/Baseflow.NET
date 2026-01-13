using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baseflow.App.Engine
{
    public static class BaseflowCalculator
    {
        // Default parameters (approximate standard values)
        public const double DefaultAlpha = 0.925;
        public const double DefaultBFImax = 0.8; 
        public const double DefaultBeta = 0.925;

        // Helper to convert List to Array for processing
        public static double[] GetQ(List<StreamflowRecord> records)
        {
            return records.Select(r => r.Q).ToArray();
        }

        #region Helper Methods

        private static int HysepInterval(double? area)
        {
            double N;
            if (area == null)
            {
                N = 5;
            }
            else
            {
                // N = A^0.2 (A in square miles). 
                // area input assumed km^2 based on python doc "basin area in km^2".
                // Python: N = np.power(0.3861022 * area, 0.2) -> 0.386... is conversion factor km2 to mi2? 
                // 1 km2 = 0.386102 sq mi. Yes.
                N = Math.Pow(0.3861022 * area.Value, 0.2);
            }
            
            double inN = Math.Ceiling(2 * N);
            if (inN % 2 == 0)
            {
                inN = Math.Ceiling(2 * N) - 1;
            }
            // inN is the odd integer between 3 and 11 nearest to 2N
            inN = Math.Max(3, Math.Min(inN, 11));
            return (int)inN;
        }

        private static double[] LinearInterpolation(double[] Q, int[] idx_turn)
        {
            double[] b = new double[Q.Length];
            int n = 0;
            // Python: for i in range(idx_turn[0], idx_turn[-1] + 1):
            // Warning: Loop needs to be careful about bounds
            if (idx_turn.Length == 0) return b;

            for (int i = idx_turn[0]; i <= idx_turn[idx_turn.Length - 1]; i++)
            {
                 if (n < idx_turn.Length - 1 && i == idx_turn[n + 1])
                 {
                     n++;
                     b[i] = Q[i];
                 }
                 else if (n < idx_turn.Length - 1)
                 {
                     // Interpolate
                     // b[i] = Q[idx_turn[n]] + (Q[idx_turn[n+1]] - Q[idx_turn[n]]) / (idx_turn[n+1] - idx_turn[n]) * (i - idx_turn[n])
                     double slope = (Q[idx_turn[n + 1]] - Q[idx_turn[n]]) / (double)(idx_turn[n + 1] - idx_turn[n]);
                     b[i] = Q[idx_turn[n]] + slope * (i - idx_turn[n]);
                 }
                 else
                 {
                     // Should be at the end point
                     b[i] = Q[i]; 
                 }

                 if (b[i] > Q[i]) b[i] = Q[i];
            }
            return b;
        }

        #endregion

        #region Methods

        public static double[] CalculateLH(double[] Q, double beta = DefaultBeta)
        {
            int n = Q.Length;
            double[] b = new double[n];
            
            // First pass
            b[0] = Q[0];
            for (int i = 0; i < n - 1; i++)
            {
                b[i + 1] = beta * b[i] + (1 - beta) / 2 * (Q[i] + Q[i + 1]);
                if (b[i + 1] > Q[i + 1]) b[i + 1] = Q[i + 1];
            }

            // Second pass (backward)
            double[] b1 = (double[])b.Clone();
            for (int i = n - 2; i >= 0; i--)
            {
                b[i] = beta * b[i + 1] + (1 - beta) / 2 * (b1[i + 1] + b1[i]);
                if (b[i] > b1[i]) b[i] = b1[i];
            }
            
            return b;
        }

        public static double[] CalculateUKIH(double[] Q, double[] b_LH)
        {
            // UKIH (1980)
            int N = 5;
            int block_end = (Q.Length / N) * N;
            
            // Find min indices in blocks of N
            List<int> idx_min = new List<int>();
            for (int i = 0; i < block_end; i += N)
            {
                // Find min in Q[i : i+N]
                int minIdx = -1;
                double minVal = double.MaxValue;
                for (int j = 0; j < N; j++)
                {
                    if (Q[i + j] < minVal)
                    {
                        minVal = Q[i + j];
                        minIdx = i + j;
                    }
                }
                idx_min.Add(minIdx);
            }

            // Turning points test: 0.9 * Q[min[i+1]] < Q[min[i]] AND 0.9 * Q[min[i+1]] < Q[min[i+2]]
            List<int> idx_turn = new List<int>();
            for (int i = 0; i < idx_min.Count - 2; i++)
            {
                if ((0.9 * Q[idx_min[i + 1]] < Q[idx_min[i]]) && 
                    (0.9 * Q[idx_min[i + 1]] < Q[idx_min[i + 2]]))
                {
                    idx_turn.Add(idx_min[i + 1]);
                }
            }

            if (idx_turn.Count < 3) return new double[Q.Length]; // Too few points

            double[] b = LinearInterpolation(Q, idx_turn.ToArray());
            
            // Fill edges with LH
            for (int i = 0; i < idx_turn[0]; i++) b[i] = b_LH[i];
            for (int i = idx_turn[idx_turn.Count - 1] + 1; i < Q.Length; i++) b[i] = b_LH[i];

            return b;
        }

        public static double[] CalculateLocalMin(double[] Q, double[] b_LH, double? area = null)
        {
            int inN = HysepInterval(area);
            int half = (inN - 1) / 2;
            List<int> idx_turn = new List<int>();

            for (int i = half; i < Q.Length - half; i++)
            {
                // Check if Q[i] is min in window [i-half, i+half]
                // Note: The python window is [i-half : i+half+1] (inclusive of end in logic)
                int start = i - half;
                int end = i + half; 
                bool isMin = true;
                for (int j = start; j <= end; j++)
                {
                    if (Q[j] < Q[i]) { isMin = false; break; }
                }
                if (isMin) idx_turn.Add(i);
            }

            if (idx_turn.Count < 3) return new double[Q.Length]; 

            double[] b = LinearInterpolation(Q, idx_turn.ToArray());
            
            for (int i = 0; i < idx_turn[0]; i++) b[i] = b_LH[i];
            for (int i = idx_turn[idx_turn.Count - 1] + 1; i < Q.Length; i++) b[i] = b_LH[i];

            return b;
        }

        public static double[] CalculateFixed(double[] Q, double? area = null)
        {
            int inN = HysepInterval(area);
            double[] b = new double[Q.Length];
            int n = Q.Length / inN;

            for (int i = 0; i < n; i++)
            {
                int start = inN * i;
                int end = inN * (i + 1);
                double minVal = double.MaxValue;
                for (int j = start; j < end; j++) if (Q[j] < minVal) minVal = Q[j];
                
                for (int j = start; j < end; j++) b[j] = minVal;
            }
            
            if (n * inN != Q.Length)
            {
                int start = n * inN;
                double minVal = double.MaxValue;
                for (int j = start; j < Q.Length; j++) if (Q[j] < minVal) minVal = Q[j];
                for (int j = start; j < Q.Length; j++) b[j] = minVal;
            }
            return b;
        }

        public static double[] CalculateSlide(double[] Q, double? area = null)
        {
            int inN = HysepInterval(area);
            double[] b = new double[Q.Length];
            int half = (inN - 1) / 2;

            // Middle
            for (int i = half; i < Q.Length - half; i++)
            {
                double minVal = double.MaxValue;
                for (int j = i - half; j <= i + half; j++) // window size inN
                {
                    if (Q[j] < minVal) minVal = Q[j];
                }
                b[i] = minVal; 
            }
            
            // Edges
            // Python: b[:(inN-1)/2] = min(Q[:(inN-1)/2])
            double minStart = double.MaxValue;
            for(int j=0; j<half; j++) if(Q[j]<minStart) minStart=Q[j];
            for(int j=0; j<half; j++) b[j] = minStart;

            double minEnd = double.MaxValue;
            for(int j=Q.Length - half; j<Q.Length; j++) if(Q[j]<minEnd) minEnd=Q[j];
            for(int j=Q.Length - half; j<Q.Length; j++) b[j] = minEnd;

            return b;
        }

        public static double[] CalculateChapman(double[] Q, double a = DefaultAlpha)
        {
            double[] b = new double[Q.Length];
            b[0] = Q[0]; // Initial Q0
            
            for (int i = 0; i < Q.Length - 1; i++)
            {
                b[i + 1] = (3 * a - 1) / (3 - a) * b[i] + (1 - a) / (3 - a) * (Q[i + 1] + Q[i]);
                if (b[i + 1] > Q[i + 1]) b[i + 1] = Q[i + 1];
            }
            return b;
        }

        public static double[] CalculateCM(double[] Q, double a = DefaultAlpha)
        {
            double[] b = new double[Q.Length];
            b[0] = Q[0];

            for (int i = 0; i < Q.Length - 1; i++)
            {
                b[i + 1] = a / (2 - a) * b[i] + (1 - a) / (2 - a) * Q[i + 1];
                 if (b[i + 1] > Q[i + 1]) b[i + 1] = Q[i + 1];
            }
            return b;
        }

        public static double[] CalculateBoughton(double[] Q, double a = DefaultAlpha, double C = 0.1) 
        {
            // Default C is arbitrary here, Python calibrates it. 0.1 is a reasonable start?
            // "C = param_calibrate(... boughton ...)"
            double[] b = new double[Q.Length];
            b[0] = Q[0];
            
            for (int i = 0; i < Q.Length - 1; i++)
            {
                b[i + 1] = a / (1 + C) * b[i] + C / (1 + C) * Q[i + 1];
                if (b[i + 1] > Q[i + 1]) b[i + 1] = Q[i + 1];
            }
            return b;
        }

        public static double[] CalculateFurey(double[] Q, double a = DefaultAlpha, double A = 0.1)
        {
             double[] b = new double[Q.Length];
             b[0] = Q[0];
             for(int i=0; i<Q.Length -1; i++)
             {
                 b[i+1] = (a - A * (1-a)) * b[i] + A * (1-a) * Q[i];
                 if (b[i + 1] > Q[i + 1]) b[i + 1] = Q[i + 1];
             }
             return b;
        }

        public static double[] CalculateEckhardt(double[] Q, double a = DefaultAlpha, double BFImax = DefaultBFImax)
        {
            double[] b = new double[Q.Length];
            b[0] = Q[0];
            for (int i = 0; i < Q.Length - 1; i++)
            {
                 // b[i+1] = ((1-BFImax)*a*b[i] + (1-a)*BFImax*Q[i+1]) / (1-a*BFImax)
                 b[i+1] = ((1 - BFImax) * a * b[i] + (1 - a) * BFImax * Q[i + 1]) / (1 - a * BFImax);
                 if (b[i + 1] > Q[i + 1]) b[i + 1] = Q[i + 1];
            }
            return b;
        }

         public static double[] CalculateEWMA(double[] Q, double e = 0.05) 
         {
             // Python calls param_calibrate for 'e' (smoothing). range 0.0001 to 0.1
             double[] b = new double[Q.Length];
             b[0] = Q[0];
             for (int i = 0; i < Q.Length - 1; i++)
             {
                 b[i + 1] = (1 - e) * b[i] + e * Q[i + 1];
                  if (b[i + 1] > Q[i + 1]) b[i + 1] = Q[i + 1];
             }
             return b;
         }

          public static double[] CalculateWillems(double[] Q, double a = DefaultAlpha, double w = 0.5)
          {
              double[] b = new double[Q.Length];
              b[0] = Q[0];
              double v = (1 - w) * (1 - a) / (2 * w);

              for (int i = 0; i < Q.Length - 1; i++)
              {
                  b[i+1] = (a - v) / (1 + v) * b[i] + v / (1 + v) * (Q[i] + Q[i+1]);
                  if (b[i + 1] > Q[i + 1]) b[i + 1] = Q[i + 1];
              }
              return b;
          }

        #endregion
    }
}
