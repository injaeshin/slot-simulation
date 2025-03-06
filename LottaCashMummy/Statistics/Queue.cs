// using System.Collections.Concurrent;

// namespace LottaCashMummy.Statistics;

// public enum QueueType
// {
//     Base,
//     Feature,
// }

// public interface IQueueItem
// {
//     QueueType QueueType { get; }
// }

// public class Queue
// {
//     private readonly ConcurrentQueue<IQueueItem> queue = new();

//     public void Enqueue(IQueueItem item)
//     {
//         queue.Enqueue(item);
//     }

//     public IQueueItem? Dequeue()
//     {
//         if (queue.TryDequeue(out var item))
//         {
//             return item;
//         }

//         return null;
//     }

//     public void Clear()
//     {
//         queue.Clear();
//     }

//     public int Count => queue.Count;
// }
