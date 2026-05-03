using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Constants;

namespace GTPatcher_Launcher.Utilities
{
    public static class DownloadHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static int DownloadManifest(ulong manifestId, string directory, string steamUsername, string branch)
        {
            try
            {
                var fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "DepotDownloader.exe" : "DepotDownloader";
                var arguments = $"-app {APP_ID} -depot {DEPOT_ID} -manifest {manifestId} -branch {branch} -username {steamUsername} -remember-password -dir \"{directory}\"";

                var proc = Process.Start(fileName, arguments);
                if (proc == null) return 1;
                proc.WaitForExit();
                return proc.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading manifest: {ex.Message}");
                return 1;
            }
        }

        public static async Task<int> DownloadUrl(string directory, string url)
        {
            try
            {
                var zipPath = Path.Combine(directory, "game.zip");
                using (var response = await _httpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(zipPath, FileMode.Create))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, directory);
                File.Delete(zipPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error downloading URL: {e.Message}");
                return 1;
            }

            try
            {
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    if (Directory.GetFiles(dir, "*.exe").Length > 0)
                    {
                        CopyFolder(dir, directory);
                        Directory.Delete(dir, true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during directory cleanup: {e.Message}");
                return 1;
            }

            return 0;
        }
        
        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(sourceFolder))
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }
    }
}
