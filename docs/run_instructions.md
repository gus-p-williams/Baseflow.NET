# How to Run Baseflow.NET in Visual Studio

## Prerequisites
- **Visual Studio 2022** (Community, Professional, or Enterprise)
- **.NET 8 SDK** installed (The installer should have selected this, or you can install it separately).

## Step-by-Step Instructions

1.  **Open the Solution**
    - Open Visual Studio.
    - Click **File > Open > Project/Solution**.
    - Navigate to your repository folder: `g:\GIT_Repo\Baseflow.NET`.
    - Select the file named **`Baseflow.sln`** and click **Open**.

2.  **Verify the Startup Project**
    - In the **Solution Explorer** (usually on the right side), look for **`Baseflow.App`**.
    - Right-click on **`Baseflow.App`** and select **"Set as Startup Project"**. (It might already be bold, which means it is selected).

3.  **Run the Application**
    - Look for the green triangular "Play" button in the top toolbar. It likely says **"Baseflow.App"**.
    - Click that button, or simply press the **F5** key on your keyboard.
    - Visual Studio will build the project (this might take a moment the first time).

4.  **Troubleshooting**
    - If you see an error about "NuGet packages", look for the **Solution Explorer**, right-click on the "Solution 'Baseflow'", and select **"Restore NuGet Packages"**.
    - If the window opens but closes immediately, ensure you are running in "Debug" execution (F5) and not just building.

## Using the App
- Once the window appears, click **"Load CSV"**.
- Navigate to a CSV file with `Date` and `Q` columns.
- Select checks boxes for methods (e.g., "Lyne-Hollick").
- Click **"Run Analysis"**.

# How to Run in Visual Studio Code (VS Code)

Based on your logs, you are using VS Code! Here is how to run it there:

1.  **Open the "Run and Debug" View**
    - Click the **Play Icon** with a bug on the left sidebar (or press `Ctrl+Shift+D`).

2.  **Start Debugging**
    - Click the **"Run and Debug"** button.
    - Select **"C#"** or **".NET 8+"** if prompted.
    - Select the **"Baseflow.App"** project if asked.
    - The app should compile and launch.

3.  **Alternative: C# Dev Kit**
    - Click the **Solution Explorer** icon (usually at the bottom of the left sidebar).
    - Right-click on the `Baseflow.App` project.
    - Select **"Debug"** > **"Start New Instance"**.
