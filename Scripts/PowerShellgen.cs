using System.IO;

namespace Galadarbs_IT23033.Scripts
{
    internal class PowerShellgen
    {
        public static void GeneratePs1File(IEnumerable<string> SelectedOptions, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(" echo test");
                foreach (var option in SelectedOptions)
                {
                    writer.WriteLine($":: Action for {option}");
                    switch (option)
                    {
                        case "Disable Customer Experience Program":

                            break;
                        case "Remove OneDrive":
                            break;
                        default:
                            writer.WriteLine($":: Unknown option: {option}");
                            break;
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}
