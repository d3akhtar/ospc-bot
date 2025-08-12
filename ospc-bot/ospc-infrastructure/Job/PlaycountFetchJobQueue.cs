using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using OSPC.Infrastructure.Http;

namespace OSPC.Infrastructure.Job
{
    public class PlaycountFetchJobQueue
    {
        private HashSet<int> _servicedUserIds = new();
        private readonly Channel<int> _userIdChannel;
        private Queue<string> _servicedUsernameQueue = new();
        private readonly ILogger<PlaycountFetchJobQueue> _logger;
        private readonly IOsuWebClient _osuWebClient;
        private readonly CancellationTokenSource _cts = new();

        public PlaycountFetchJobQueue(ILogger<PlaycountFetchJobQueue> logger, IOsuWebClient osuWebClient)
        {
            _logger = logger;
            _osuWebClient = osuWebClient;
            _userIdChannel = Channel.CreateUnbounded<int>();
            Task.Run(LoadBeatmapPlaycountsAsync);
        }

        public async Task EnqueueAsync(int userId, string username) {
            _logger.LogInformation("Added {Username} to the queue, id: {UserId}", username, userId);
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
                    _logger.LogInformation("Servicing user with id: {UserId}", userId);
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
