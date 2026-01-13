using System;
using CsvHelper.Configuration.Attributes;

namespace Baseflow.App.Engine
{
    public class StreamflowRecord
    {
        [Index(0)]
        public DateTime Date { get; set; }
        
        [Index(1)]
        public double Q { get; set; } // Discharge
    }

    public class BaseflowResult
    {
        public DateTime Date { get; set; }
        public double Baseflow { get; set; }
    }
}
