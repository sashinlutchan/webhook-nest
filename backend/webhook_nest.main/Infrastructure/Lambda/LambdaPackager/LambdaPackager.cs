using System;
using System.Diagnostics;
using System.IO;

namespace webhook_nest.main.Infrastructure.Lambda.LambdaPackager;

public  static class LambdaPackager
{


    public static string BuildAndZipLambda(string lambdaProjectPath, string outputFolder, string zipName)
    {
        var publishDir = Path.Combine(outputFolder, "publish");

        if (Directory.Exists(publishDir))
            Directory.Delete(publishDir, true);

        Directory.CreateDirectory(publishDir);

        var psi = new ProcessStartInfo("dotnet", $"publish \"{lambdaProjectPath}\" -c Release -o \"{publishDir}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi);
        proc!.WaitForExit();

        if (proc.ExitCode != 0)
        {
            var err = proc.StandardError.ReadToEnd();
            throw new Exception($"Lambda publish failed: {err}");
        }

        var zipPath = Path.Combine(outputFolder, zipName);
        if (File.Exists(zipPath)) File.Delete(zipPath);
        System.IO.Compression.ZipFile.CreateFromDirectory(publishDir, zipPath);

        return zipPath;
    }
}

