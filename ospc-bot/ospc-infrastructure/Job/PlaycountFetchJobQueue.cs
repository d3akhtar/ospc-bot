using System.Threading.Channels;
using OSPC.Infrastructure.Http;

namespace OSPC.Infrastructure.Job
{
    public class PlaycountFetchJobQueue
    {
        private HashSet<int> _servicedUserIds = new();
        private readonly Channel<int> _userIdChannel;
        private Queue<string> _servicedUsernameQueue = new();
        private readonly IOsuWebClient _osuWebClient;
        private readonly CancellationTokenSource _cts = new();

        public PlaycountFetchJobQueue(IOsuWebClient osuWebClient)
        {
            _osuWebClient = osuWebClient;
            _userIdChannel = Channel.CreateUnbounded<int>();
            Task.Run(LoadBeatmapPlaycountsAsync);
        }

        public async Task EnqueueAsync(int userId, string username) {
            Console.WriteLine($"Queued user with id: {userId}");
            await _userIdChannel.Writer.WriteAsync(userId);
            _servicedUsernameQueue.Enqueue(username);
            _servicedUserIds.Add(userId);
        }
    
        public async Task LoadBeatmapPlaycountsAsync()
        {
            while (await _userIdChannel.Reader.WaitToReadAsync(_cts.Token))
            {
                while (_userIdChannel.Reader.TryRead(out int userId))
                {
                    Console.WriteLine($"Servicing user with id: {userId}");
                    await _osuWebClient.LoadUserPlayedMaps(userId);
                    _servicedUserIds.Remove(userId);
                    _servicedUsernameQueue.Dequeue();
                }
            }
        }

        public bool MapsBeingFetched(int userId) => _servicedUserIds.Contains(userId);
        public Queue<string> QueuedUsers => _servicedUsernameQueue;

        public void StopLoading() => _cts.Cancel();
    }
}