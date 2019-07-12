using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HowToVideoFiles.BackgroundTasks
{
    public interface IBackgroundQueue
    {
        void QueueTask(Func<CancellationToken, Task> task);

        Task<Func<CancellationToken, Task>> PopQueue(CancellationToken cancellationToken);
    }

    public class BackgroundQueue : IBackgroundQueue
    {
        private ConcurrentQueue<Func<CancellationToken, Task>> Tasks;

        private SemaphoreSlim Signal;

        public BackgroundQueue()
        {
            Tasks = new ConcurrentQueue<Func<CancellationToken, Task>>();
            Signal = new SemaphoreSlim(0); 
        }

        public void QueueTask(Func<CancellationToken, Task> task)
        {
            Tasks.Enqueue(task);
            Signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> PopQueue(CancellationToken cancellationToken)
        {
            await Signal.WaitAsync(cancellationToken);
            Tasks.TryDequeue(out var task);

            return task;
        }
    }
}
