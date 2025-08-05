using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{
    using System;
    using System.IO;

    public static class Cleanup
    {
        public static void DeleteProjectTempFolders()
        {
            string tempRoot = Path.GetTempPath();
            string prefix = "Project_";

            try
            {
                foreach (string dir in Directory.GetDirectories(tempRoot, prefix + "*"))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        Console.WriteLine("Deleted: " + dir);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting '{dir}': {ex.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error searching temp directory: " + e.Message);
            }
        }

    }
}
