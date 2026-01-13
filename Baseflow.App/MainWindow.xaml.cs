using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Baseflow.App.Engine;
using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using Microsoft.Win32;

namespace Baseflow.App
{
    public partial class MainWindow : Window
    {
        private List<StreamflowRecord>? _records;
        private Dictionary<string, double[]> _results = new Dictionary<string, double[]>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadCsv_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    FilePathText.Text = openFileDialog.FileName;
                    using (var reader = new StreamReader(openFileDialog.FileName))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        MissingFieldFound = null,
                        HeaderValidated = null
                    }))
                    {
                        _records = csv.GetRecords<StreamflowRecord>().ToList();
                    }
                    
                    MessageBox.Show($"Loaded {_records.Count} records.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Initial Plot of Streamflow
                    WpfPlot1.Plot.Clear();
                    var dates = _records.Select(x => x.Date).ToArray();
                    var q = _records.Select(x => x.Q).ToArray();
                    
                    var sig = WpfPlot1.Plot.Add.Scatter(dates, q);
                    sig.Label = "Streamflow";
                    sig.LineWidth = 2;
                    
                    WpfPlot1.Plot.Axes.DateTimeTicksBottom();
                    WpfPlot1.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            if (_records == null || _records.Count == 0)
            {
                MessageBox.Show("Please load a CSV file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _results.Clear();
            var Q = BaseflowCalculator.GetQ(_records);
            var dates = _records.Select(r => r.Date).ToArray();
            
            // Calculate Baseflow (LH is often a dependency for others)
            double[]? b_LH = null; 
            if (ChkMethod_LH.IsChecked == true || ChkMethod_UKIH.IsChecked == true || ChkMethod_LocalMin.IsChecked == true)
            {
                 // We always calc LH if needed for dependency, but only store if checked?
                 // Actually calculating is cheap.
                 b_LH = BaseflowCalculator.CalculateLH(Q);
            }

            // For methods needing 'a' (recession coef), we use default or calculate?
            // Python uses 'recession_coefficient' function. We'll use default 0.925 for now as strict implementation is complex.
            double a = BaseflowCalculator.DefaultAlpha;

            WpfPlot1.Plot.Clear();
            WpfPlot1.Plot.Add.Scatter(dates, Q).Label = "Streamflow";
            
            // Run Selected Methods
            if (ChkMethod_LH.IsChecked == true) AddResult("Lyne-Hollick", b_LH ?? BaseflowCalculator.CalculateLH(Q), dates);
            
            if (ChkMethod_UKIH.IsChecked == true) 
            {
                var inputLH = b_LH ?? BaseflowCalculator.CalculateLH(Q);
                AddResult("UKIH", BaseflowCalculator.CalculateUKIH(Q, inputLH), dates);
            }

            if (ChkMethod_LocalMin.IsChecked == true)
            {
                 var inputLH = b_LH ?? BaseflowCalculator.CalculateLH(Q);
                 AddResult("Local Min", BaseflowCalculator.CalculateLocalMin(Q, inputLH), dates);
            }

            if (ChkMethod_Fixed.IsChecked == true) AddResult("Fixed Interval", BaseflowCalculator.CalculateFixed(Q), dates);
            if (ChkMethod_Slide.IsChecked == true) AddResult("Sliding Interval", BaseflowCalculator.CalculateSlide(Q), dates);
            if (ChkMethod_Chapman.IsChecked == true) AddResult("Chapman", BaseflowCalculator.CalculateChapman(Q, a), dates);
            if (ChkMethod_CM.IsChecked == true) AddResult("CM", BaseflowCalculator.CalculateCM(Q, a), dates);
            if (ChkMethod_Boughton.IsChecked == true) AddResult("Boughton", BaseflowCalculator.CalculateBoughton(Q, a), dates);
            if (ChkMethod_Furey.IsChecked == true) AddResult("Furey", BaseflowCalculator.CalculateFurey(Q, a), dates);
            if (ChkMethod_Eckhardt.IsChecked == true) AddResult("Eckhardt", BaseflowCalculator.CalculateEckhardt(Q, a), dates);
            if (ChkMethod_EWMA.IsChecked == true) AddResult("EWMA", BaseflowCalculator.CalculateEWMA(Q), dates);
            if (ChkMethod_Willems.IsChecked == true) AddResult("Willems", BaseflowCalculator.CalculateWillems(Q, a), dates);

            WpfPlot1.Plot.ShowLegend();
            WpfPlot1.Plot.Axes.DateTimeTicksBottom();
            WpfPlot1.Refresh();
        }

        private void AddResult(string name, double[] data, DateTime[] dates)
        {
            _results[name] = data;
            var sig = WpfPlot1.Plot.Add.Scatter(dates, data);
            sig.Label = name;
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (_records == null || _results.Count == 0)
            {
                 MessageBox.Show("No results to export.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                 return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = "baseflow_results.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var writer = new StreamWriter(saveFileDialog.FileName))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // Dynamic writing of columns
                    // Write Header
                    csv.WriteField("Date");
                    csv.WriteField("Streamflow");
                    foreach (var key in _results.Keys) csv.WriteField(key);
                    csv.NextRecord();

                    // Write Rows
                    for (int i = 0; i < _records.Count; i++)
                    {
                        csv.WriteField(_records[i].Date);
                        csv.WriteField(_records[i].Q);
                        foreach (var key in _results.Keys)
                        {
                            csv.WriteField(_results[key][i]);
                        }
                        csv.NextRecord();
                    }
                }
                MessageBox.Show("Export complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
