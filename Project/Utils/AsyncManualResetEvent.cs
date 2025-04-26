
namespace IPK
{
    /// <summary>
    /// This class is used for waiting to some event that should happen,
    /// but we should check is async, otherwise it will be big CPU load.
    /// It's moved to another class because just using TaskCompletionSource wasn't working; 
    /// The program was ending when this source was set, and to fix it, it was moved to another class.
    /// So it's like methods in this class are completed, not the part of code where this source was awaited.
    /// </summary>
    public class AsyncManualResetEvent
    {
        /// <summary>
        /// Signal that is set when an event occurs, otherwise it lets the program wait async.
        /// </summary>
        private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        /// <summary>
        /// Starts waiting.
        /// </summary>
        /// <returns> Returns task itself, needed to check if it ended or timer that was set with this signal (there always some timer that are set with this signal). </returns>
        public Task WaitAsync() => _tcs.Task;
        /// <summary>
        /// Ends tasks successfully, indicates that event happened successfully.
        /// </summary>
        public void Set()
        {
            _tcs.TrySetResult(true);
        }
        /// <summary>
        /// Resets signal so it can be used again.
        /// </summary>
        public void Reset()
        {
            while (_tcs.Task.IsCompleted)
            {
                var tcs = new TaskCompletionSource<bool>();
                if (Interlocked.CompareExchange(ref _tcs, tcs, _tcs) == _tcs)
                    break;
            }
        }
    }
}