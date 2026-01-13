# Baseflow.NET Walkthrough

## Summary
The `baseflow` Python library has been successfully ported to a .NET 8 WPF application (`Baseflow.App`). The source code is hosted in the local `Baseflow.NET` repository and pushed to GitHub.

## 1. Project Structure
- **Baseflow.sln**: Visual Studio Solution file.
- **Baseflow.App/**: Main WPF Application project.
  - **Engine/BaseflowCalculator.cs**: Core logic containing 12 baseflow separation methods (UKIH, Local, LH, etc.).
  - **MainWindow.xaml**: User interface.
  - **MainWindow.xaml.cs**: UI Logic, CSV handling, and Export.

## 2. Algorithms Implemented
Based on the `baseflow` Python library (BYU-Hydroinformatics):
- **Graphical Methods**: UKIH, Local Minimum, Fixed Interval, Sliding Interval.
- **Digital Filters**: Lyne-Hollick (LH), Chapman, CM, Boughton, Furey, Eckhardt, EWMA, Willems.

## 3. How to Run
1. **Open the Project**: Open `Baseflow.sln` in Visual Studio or VS Code.
2. **Restore Packages** (if needed):
   ```powershell
   dotnet restore
   ```
3. **Build & Run**:
   ```powershell
   dotnet run --project Baseflow.App
   ```

## 4. Usage Guide
1. **Launch App**.
2. **Load CSV**: Headers must include `Date` and `Q` (Discharge).
3. **Select Methods**: Check the boxes for the algorithms you want to compare.
4. **Run Analysis**: The interactive graph will display the original Streamflow and the calculated Baseflow(s).
5. **Export**: Save the data to a new CSV file for further analysis.
