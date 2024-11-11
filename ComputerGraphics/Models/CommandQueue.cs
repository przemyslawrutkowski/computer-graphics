using System.Collections.Concurrent;

namespace ComputerGraphics.Models
{
    public class CommandQueue
    {
        private readonly ConcurrentQueue<Func<Task>> _queue = new ConcurrentQueue<Func<Task>>();
        private bool _isProcessing = false;

        public void Enqueue(Func<Task> command)
        {
            _queue.Enqueue(command);
            ProcessQueue();
        }

        private async void ProcessQueue()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;

            while (_queue.TryDequeue(out var command))
            {
                try
                {
                    await command();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during command execution: {ex.Message}");
                }
            }

            _isProcessing = false;
        }
    }
}
