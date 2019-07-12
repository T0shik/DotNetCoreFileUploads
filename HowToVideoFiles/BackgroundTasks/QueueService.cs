using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace HowToVideoFiles.BackgroundTasks
{
    public class QueueService : BackgroundService
    {
        private IBackgroundQueue _queue;

        public QueueService(IBackgroundQueue queue)
        {
            _queue = queue;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                var task = await _queue.PopQueue(stoppingToken);

                await task(stoppingToken);
            }
        }
    }
}
