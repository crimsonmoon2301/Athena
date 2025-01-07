using System.IO;

namespace Galadarbs_IT23033.Scripts
{
    public static class BatFileGenerator
    {
        public static void GenerateBatFile(IEnumerable<string> SelectedOptions, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(":: Batch file generated on " + DateTime.Now + " via Athena PC Utility");
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
                            writer.WriteLine(":: Disable Customer Experience Program");
                            writer.WriteLine("PowerShell -ExecutionPolicy Unrestricted -Command \"reg delete 'HKLM\\Software\\Microsoft\\SQMClient' /v 'UploadDisableFlag' /f 2>$null\"");
                            writer.WriteLine("PowerShell -ExecutionPolicy Unrestricted -Command \"reg delete 'HKLM\\Software\\Policies\\Microsoft\\SQMClient\\Windows' /v 'CEIPEnable' /f 2>$null\"");
                            break;
                        case "Disable MareBackup":
                            writer.WriteLine(":: Disable application backup data gathering (`MareBackup`)");
                            writer.WriteLine(":: Disable scheduled task(s): `\\Microsoft\\Windows\\Application Experience\\MareBackup`");
                            writer.WriteLine(@"PowerShell -ExecutionPolicy Unrestricted -Command ""$taskPathPattern='\\Microsoft\\Windows\\Application Experience\\'; $taskNamePattern='MareBackup'; Write-Output 'Disabling tasks matching pattern `$taskNamePattern'; $tasks = @(Get-ScheduledTask -TaskPath $taskPathPattern -TaskName $taskNamePattern -ErrorAction Ignore); if (-Not $tasks) { Write-Output 'Skipping, no tasks matching pattern `$taskNamePattern found, no action needed.'; exit 0; }; $operationFailed = $false; foreach ($task in $tasks) { $taskName = $task.TaskName; if ($task.State -eq [Microsoft.PowerShell.Cmdletization.GeneratedTypes.ScheduledTask.StateEnum]::Disabled) { Write-Output 'Skipping, task `$taskName is already disabled, no action needed.'; continue; }; try { $task | Disable-ScheduledTask -ErrorAction Stop | Out-Null; Write-Output 'Successfully disabled task `$taskName.'; } catch { Write-Error 'Failed to disable task `$taskName: $($_.Exception.Message)'; $operationFailed = $true; }; }; if ($operationFailed) { Write-Output 'Failed to disable some tasks. Check error messages above.'; exit 1; }""");
                            break;
                        case "Disable Recall":
                            writer.WriteLine(":: Disable Recall on Copilot+ PCs");
                            writer.WriteLine("reg.exe add \"HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\WindowsAI\" /f");
                            writer.WriteLine("reg.exe add \"HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\WindowsAI\" /v \"DisableAIDataAnalysis\" /t REG_DWORD /d 1 /f");
                            writer.WriteLine("reg.exe add \"HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\Windows AI\" /v \"TurnOffSavingSnapshots\" /t REG_DWORD /d 1 /f");
                            break;
                        case "Empty Recycle Bin":
                            writer.WriteLine(":: Empty trash(Recycle Bin)");
                            writer.WriteLine("PowerShell Clear-RecycleBin -Force");
                            break;
                        case "Enable Ultimate Performance Powerplan":
                            writer.WriteLine(":: Enable Ultimate Performance Plan (may not be visible on win 11)");
                            writer.WriteLine("PowerShell -ExecutionPolicy Unrestricted -Command powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                            break;
                        case "Disable Defender":
                            writer.WriteLine(":: Disable Defender");
                            writer.WriteLine("powershell -command 'Set-MpPreference -DisableRealtimeMonitoring $true -DisableScriptScanning $true -DisableBehaviorMonitoring $true -DisableIOAVProtection $true -DisableIntrusionPreventionSystem $true'");
                            break;
                        case "Disable Telemetry":
                            writer.WriteLine(":: Disable telemetry");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\DataCollection\" /v AllowTelemetry /t REG_DWORD /d 0 /f");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v AllowTelemetry /t REG_DWORD /d 0 /f");
                            break;
                        case "Disable Ad ID for All Users":
                            writer.WriteLine(":: Disable Advertisement ID for all users");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\AdvertisingInfo\" /v DisabledByGroupPolicy /t REG_DWORD /d 1 /f");
                            break;
                        case "Give 3D apps higher priority":
                            writer.WriteLine(":: Give 3D apps like multimedia or games higher priority");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\" /v SystemResponsiveness /t REG_DWORD /d 0 /f");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\" /v NetworkThrottlingIndex /t REG_DWORD /d 10 /f");
                            break;
                        case "Give GPU higher priority":
                            writer.WriteLine(":: Give Graphics (GPU) higher priority");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\Tasks\\Games\" /v \"GPU Priority\" /t REG_DWORD /d 8 /f");
                            break;
                        case "Give CPU higher priority":
                            writer.WriteLine(":: Give Processor (CPU) higher priority");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\Tasks\\Games\" /v Priority /t REG_DWORD /d 6 /f");
                            break;
                        case "Give Multimedia higher priority":
                            writer.WriteLine(":: Give games or demanding multimedia apps higher priority in the scheduler");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\Tasks\\Games\" /v \"Scheduling Category\" /t REG_SZ /d \"High\" /f");
                            break;
                        case "Disable Cortana":
                            writer.WriteLine(":: Disable Microsoft Cortana");
                            writer.WriteLine("reg.exe add \"HKLM\\Software\\Policies\\Microsoft\\Windows\\Windows Search\" /v AllowCortana /t REG_DWORD /d 0 /f");
                            break;
                        case "Disable Location Tracking":
                            writer.WriteLine(":: Disable Location trackers");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CapabilityAccessManager\\ConsentStore\\location\" /v Value /t REG_SZ /d Deny /f");
                            writer.WriteLine("reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Sensor\\Overrides\\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}\" /v SensorPermissionState /t REG_DWORD /d 0 /f");
                            writer.WriteLine("reg.exe add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\lfsvc\\Service\\Configuration\" /v Status /t REG_DWORD /d 0 /f");
                            writer.WriteLine("reg.exe add \"HKLM\\SYSTEM\\Maps\" /v AutoUpdateEnabled /t REG_DWORD /d 0 /f");
                            break;
                        case "Disable Copilot":
                            writer.WriteLine(":: Disable Copilot, which comes installed in 10 and 11");
                            writer.WriteLine("reg.exe add \"HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Runonce\" /v \"UninstallCopilot\" /t REG_SZ /d \"\" /f");
                            writer.WriteLine("reg.exe add \"HKEY_CURRENT_USER\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsCopilot\" /v TurnOffWindowsCopilot /t REG_DWORD /d 1 /f");
                            break;
                        case "Disable Security":
                            writer.WriteLine(":: Disables Security. Highly dangerous!");
                            writer.WriteLine("reg.exe add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Sense\" /v Start /t REG_DWORD /d 4 /f");
                            writer.WriteLine("reg.exe add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\WdBoot\" /v Start /t REG_DWORD /d 4 /f");
                            writer.WriteLine("reg.exe add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\WdFilter\" /v Start /t REG_DWORD /d 4 /f");
                            writer.WriteLine("reg.exe add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\WdNisDrv\" /v Start /t REG_DWORD /d 4 /f");
                            writer.WriteLine("reg.exe add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\WdNisSvc\" /v Start /t REG_DWORD /d 4 /f");
                            writer.WriteLine("reg.exe add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\WinDefend\" /v Start /t REG_DWORD /d 4 /f");
                            break;
                        case "Clear pagefile after shutdown":
                            writer.WriteLine(":: Clears pagefile after shutdown. May slow down the shutdown, but increases performance under extreme load.");
                            writer.WriteLine("reg.exe add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v ClearPageFileAtShutdown /t REG_DWORD /d 0 /f");
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
