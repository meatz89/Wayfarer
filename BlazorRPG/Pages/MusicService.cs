using Microsoft.JSInterop;
using Timer = System.Timers.Timer;

namespace BlazorRPG.Components
{
    public class MusicService
    {
        private readonly IJSRuntime _jsRuntime;
        private List<Track> _queue = new List<Track>();
        private int _currentTrackIndex = -1;
        private bool _isPlaying = false;
        private TimeSpan _currentPosition = TimeSpan.Zero;
        private Timer _positionUpdateTimer;

        public event Action OnQueueChanged;
        public event Action OnPlaybackStateChanged;
        public event Action<TimeSpan> OnPositionChanged;

        public MusicService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _positionUpdateTimer = new Timer(1000);
            _positionUpdateTimer.Elapsed += async (sender, e) =>
            {
                if (_isPlaying)
                {
                    _currentPosition = _currentPosition.Add(TimeSpan.FromSeconds(1));
                    OnPositionChanged?.Invoke(_currentPosition);

                    // Check if track ended
                    if (_currentPosition >= CurrentTrack?.Duration)
                    {
                        await PlayNextTrackAsync();
                    }
                }
            };
            _positionUpdateTimer.Start();
        }

        public List<Track> Queue
        {
            get
            {
                return _queue;
            }
        }

        public Track CurrentTrack
        {
            get
            {
                return _currentTrackIndex >= 0 && _currentTrackIndex < _queue.Count ? _queue[_currentTrackIndex] : null;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
        }

        public TimeSpan CurrentPosition
        {
            get
            {
                return _currentPosition;
            }
        }

        public int CurrentTrackIndex
        {
            get
            {
                return _currentTrackIndex;
            }
        }

        public void ClearQueue()
        {
            _queue.Clear();
            _currentTrackIndex = -1;
            _isPlaying = false;
            _currentPosition = TimeSpan.Zero;
            OnQueueChanged?.Invoke();
            OnPlaybackStateChanged?.Invoke();
            OnPositionChanged?.Invoke(_currentPosition);
        }

        public void EnqueueTrack(Track track)
        {
            _queue.Add(track);
            OnQueueChanged?.Invoke();
        }

        public void EnqueueTracks(IEnumerable<Track> tracks)
        {
            _queue.AddRange(tracks);
            OnQueueChanged?.Invoke();
        }

        public async Task PlayAsync()
        {
            if (_queue.Count == 0 || _currentTrackIndex < 0)
            {
                if (_queue.Count > 0)
                {
                    _currentTrackIndex = 0;
                }
                else
                {
                    return;
                }
            }

            _isPlaying = true;
            await _jsRuntime.InvokeVoidAsync("musicPlayer.play", CurrentTrack.FilePath);
            OnPlaybackStateChanged?.Invoke();
        }

        public async Task PauseAsync()
        {
            _isPlaying = false;
            await _jsRuntime.InvokeVoidAsync("musicPlayer.pause");
            OnPlaybackStateChanged?.Invoke();
        }

        public async Task PlayNextTrackAsync()
        {
            if (_queue.Count == 0)
                return;

            _currentPosition = TimeSpan.Zero;

            if (_currentTrackIndex < _queue.Count - 1)
            {
                _currentTrackIndex++;
            }
            else
            {
                // Loop back to the first track
                _currentTrackIndex = 0;
            }

            if (_isPlaying)
            {
                await _jsRuntime.InvokeVoidAsync("musicPlayer.play", CurrentTrack.FilePath);
            }

            OnQueueChanged?.Invoke();
            OnPlaybackStateChanged?.Invoke();
            OnPositionChanged?.Invoke(_currentPosition);
        }

        public async Task SetPositionAsync(TimeSpan position)
        {
            _currentPosition = position;
            await _jsRuntime.InvokeVoidAsync("musicPlayer.setPosition", position.TotalSeconds);
            OnPositionChanged?.Invoke(_currentPosition);
        }

        public async Task StartPlayingQueueAsync()
        {
            if (_queue.Count > 0)
            {
                _currentTrackIndex = 0;
                _currentPosition = TimeSpan.Zero;
                await PlayAsync();
            }
        }

        public async Task StopCurrentTrackAsync()
        {
            _isPlaying = false;
            _currentPosition = TimeSpan.Zero;
            await _jsRuntime.InvokeVoidAsync("musicPlayer.stop");
            OnPlaybackStateChanged?.Invoke();
            OnPositionChanged?.Invoke(_currentPosition);
        }
    }
}