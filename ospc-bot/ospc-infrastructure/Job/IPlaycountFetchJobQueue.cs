namespace OSPC.Infrastructure.Job
{
    public interface IPlaycountFetchJobQueue
    {
        public Task EnqueueAsync(int userId, string username);
        public Task LoadBeatmapPlaycountsAsync();
        public bool MapsBeingFetched(int userId);
        public Queue<string> GetQueuedUsers();
        public void StopLoading();
    }
}