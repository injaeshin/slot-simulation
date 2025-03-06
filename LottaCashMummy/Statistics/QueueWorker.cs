// using LottaCashMummy.Statistics.Model;

// namespace LottaCashMummy.Statistics;

// public class QueueWorker : IDisposable
// {
//     private readonly Queue queue = new();
//     private readonly CancellationTokenSource _cancellationTokenSource = new();
//     private readonly Task _processTask;
//     private bool _disposed = false;
//     private readonly SemaphoreSlim _processSemaphore = new SemaphoreSlim(1, 1);

//     private readonly StatsStorage storage;

//     public QueueWorker(StatsStorage storage)
//     {
//         this.storage = storage;
//         _processTask = Task.Factory.StartNew(ProcessQueueAsync, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

//         _processTask.ContinueWith(t =>
//         {
//             if (t.IsFaulted)
//             {
//                 Console.WriteLine($"Error in QueueWorker: {t.Exception}");
//             }
//         });

//     }

//     public int QueueCount => queue.Count;

//     public async Task Add(IQueueItem item)
//     {
//         queue.Enqueue(item);
//         try
//         {
     
//         }
//         finally
//         {
//             _processSemaphore.Release();
//         }

//     private async Task ProcessQueueAsync()
//     {
//         while (queue.Count > 0)
//         {
//             var item = queue.Dequeue();
//             if (item == null)
//             {
//                 continue;
//             }

//             if (item.QueueType == QueueType.Feature)
//             {
//                 if (item is FeatureGameLogModel featureGameLogModel)
//                 {
//                     storage.AddFeatureGameLog(featureGameLogModel);
//                 }
//             }
//             else
//             {
//                 if (item is BaseGameLogModel baseGameLogModel)
//                 {
//                     storage.AddBaseGameLog(baseGameLogModel);
//                 }
//             }
//         }
//     }
// }