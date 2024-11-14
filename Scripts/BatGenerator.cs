using System.IO;

namespace Galadarbs_IT23033.Scripts
{
    public static class BatFileGenerator
    {
        public static void GenerateBatFile(IEnumerable<string> SelectedOptions, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(":: Batch file generated on " + DateTime.Now + " via System Sweeper 9000 PC Utility");
                writer.WriteLine("::----------------------------");
                // Ensures that the script has admin rights. How else can you do changes to the system without them? This ain't powershell directly.
                writer.WriteLine("@echo off");
                writer.WriteLine(":: Ensure admin privileges");
                writer.WriteLine("fltmc >nul 2>&1 || (");
                writer.WriteLine("    echo Administrator privileges are required.");
                writer.WriteLine("    PowerShell Start -Verb RunAs '%0' 2> nul || (");
                writer.WriteLine("        echo Right-click on the script and select \"Run as administrator\".");
                writer.WriteLine("        pause & exit 1");
                writer.WriteLine("    )");
                writer.WriteLine("    exit 0");
                writer.WriteLine(")");
                writer.WriteLine(":: Initialize environment");
                writer.WriteLine("setlocal EnableExtensions DisableDelayedExpansion");
                writer.WriteLine();

                // Insert commands based on selected options
                foreach (var option in SelectedOptions)
                {
                    writer.WriteLine($":: Action for {option}");
                    switch (option)
                    {
                        case "Disable Customer Experience Program":
                            writer.WriteLine("echo ---Disable Customer Experience Improvement Program");
                            writer.WriteLine("PowerShell -ExecutionPolicy Unrestricted -Command \"reg delete 'HKLM\\Software\\Microsoft\\SQMClient' /v 'UploadDisableFlag' /f 2>$null\"");
                            writer.WriteLine("PowerShell -ExecutionPolicy Unrestricted -Command \"reg delete 'HKLM\\Software\\Policies\\Microsoft\\SQMClient\\Windows' /v 'CEIPEnable' /f 2>$null\"");
                            writer.WriteLine(":: ----------------------------------------------------------");
                            break;
                        case "Remove OneDrive":
                            break;
                        default:
                            writer.WriteLine($":: Unknown option: {option}");
                            break;
                    }
                    writer.WriteLine();
                }

                // Finish the script
                writer.WriteLine(":: Pause the script to view the final state ");
                writer.WriteLine("pause");
                writer.WriteLine(":: Restore previous environment settings");
                writer.WriteLine("endlocal");
                writer.WriteLine(":: Exit the script successfully");
                writer.WriteLine("exit /b 0");
            }
        }
    }
}
