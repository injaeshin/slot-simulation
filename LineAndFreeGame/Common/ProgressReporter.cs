
using System.Diagnostics;

namespace LineAndFreeGame.Common;

public class ProgressReporter : IDisposable
{
    private readonly Timer progressTimer;
    private readonly long totalIterations;
    private long currentProgress;

    public ProgressReporter(long totalIterations, Stopwatch sw)
    {
        this.totalIterations = totalIterations;
        this.progressTimer = new Timer(_ => ReportProgress(sw), null, 0, 3000);
    }

    public void UpdateProgress(long progress)
    {
        Interlocked.Add(ref currentProgress, progress);
    }

    private void ReportProgress(Stopwatch sw)
    {
        var progress = Interlocked.Read(ref currentProgress);
        var percentage = (double)progress / totalIterations * 100;
        var elapsed = sw.ElapsedMilliseconds / 1000.0;
        var spinsPerSec = progress / elapsed;
        var currentMemory = GC.GetTotalMemory(false) / 1024 / 1024;

        Console.WriteLine($"{percentage:F2}% ({progress:N0} spins, {spinsPerSec:N0} spins/sec) / [메모리] 현재 사용량: {currentMemory}MB");
    }

    public void PrintFinalStats(Stopwatch sw)
    {
        var totalSeconds = sw.ElapsedMilliseconds / 1000.0;
        var avgSpinsPerSec = totalIterations / totalSeconds;

        Console.WriteLine();
        Console.WriteLine($"Simulation completed in {totalSeconds:F2} seconds");
        Console.WriteLine($"Average speed: {avgSpinsPerSec:N0} spins/sec");
        Console.WriteLine($"Total elapsed time: {sw.ElapsedMilliseconds:N0}ms");
    }

    public void Dispose() => this.progressTimer.Dispose();
}


