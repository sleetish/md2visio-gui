using md2visio.GUI.Forms;

namespace md2visio.GUI;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Ensure COM thread mode
        System.Threading.Thread.CurrentThread.SetApartmentState(System.Threading.ApartmentState.STA);
        
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        // Set application appearance
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        // Start main window
        Application.Run(new MainForm());
    }    
}