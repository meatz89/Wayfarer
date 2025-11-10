using Microsoft.JSInterop;

public class MusicService
{
private readonly IJSRuntime _jsRuntime;
private List<Track> _queue = new List<Track>();
private int _currentTrackIndex = -1;
private bool _isPlaying = false;
private TimeSpan _currentPosition = TimeSpan.Zero;

// Events removed - UI components should poll state or use direct updates
// Architecture principle: NO events outside Razor code-behind
private int _stateVersion = 0; // Increment on any state change for efficient polling

public MusicService(IJSRuntime jsRuntime)
{
    _jsRuntime = jsRuntime;
}

public List<Track> Queue => _queue;

public Track CurrentTrack => _currentTrackIndex >= 0 && _currentTrackIndex < _queue.Count
    ? _queue[_currentTrackIndex]
    : null;

public bool IsPlaying => _isPlaying;

public TimeSpan CurrentPosition => _currentPosition;

public int CurrentTrackIndex => _currentTrackIndex;

public int StateVersion => _stateVersion; // For efficient change detection

public List<Track> LoadTracksFromFolder(string folderName)
{
    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "music", folderName);
    if (!Directory.Exists(folderPath))
        return new List<Track>();

    string[] files = Directory.GetFiles(folderPath, "*.mp3");

    return files.Select(file =>
    {
        string fileName = Path.GetFileName(file);
        return new Track(
            Path.GetFileNameWithoutExtension(file),
            $"music/{folderName}/{fileName}",
            TimeSpan.FromMinutes(3),
            new List<string> { folderName.ToLower() }
        );
    }).ToList();
}

public void ClearQueue()
{
    _queue.Clear();
    _currentTrackIndex = -1;
    _isPlaying = false;
    _currentPosition = TimeSpan.Zero;

    _stateVersion++; // Signal state change for polling
}

public void EnqueueTrack(Track track)
{
    _queue.Add(track);
    _stateVersion++; // Signal queue change
}

public void EnqueueTracks(IEnumerable<Track> tracks)
{
    _queue.AddRange(tracks);
    _stateVersion++; // Signal queue change
}

public async Task PlayAsync()
{
    if (_queue.Count == 0 || _currentTrackIndex < 0)
    {
        if (_queue.Count > 0)
            _currentTrackIndex = 0;
        else
            return;
    }

    _isPlaying = true;
    await _jsRuntime.InvokeVoidAsync("musicPlayer.play", CurrentTrack.FilePath);
    _stateVersion++; // Signal playback state change
}

public async Task PauseAsync()
{
    _isPlaying = false;
    await _jsRuntime.InvokeVoidAsync("musicPlayer.pause");
    _stateVersion++; // Signal playback state change
}

public async Task StopCurrentTrackAsync()
{
    _isPlaying = false;
    _currentPosition = TimeSpan.Zero;
    await _jsRuntime.InvokeVoidAsync("musicPlayer.stop");
    _stateVersion++; // Signal playback state change
    _stateVersion++; // Signal position change
}

public async Task PlayNextTrackAsync()
{
    if (_queue.Count == 0)
        return;

    _currentPosition = TimeSpan.Zero;

    if (_currentTrackIndex < _queue.Count - 1)
        _currentTrackIndex++;
    else
        _currentTrackIndex = 0;

    if (_isPlaying)
        await _jsRuntime.InvokeVoidAsync("musicPlayer.play", CurrentTrack.FilePath);

    _stateVersion++; // Signal state change for polling
}

public async Task StartPlayingQueueAsync()
{
    if (_queue.Count == 0)
        return;

    _currentTrackIndex = 0;
    _currentPosition = TimeSpan.Zero;
    await PlayAsync();
}

public async Task SetPositionAsync(TimeSpan position)
{
    _currentPosition = position;
    await _jsRuntime.InvokeVoidAsync("musicPlayer.setPosition", position.TotalSeconds);
    _stateVersion++; // Signal position change
}

public async Task IncrementPositionAsync(TimeSpan delta)
{
    _currentPosition += delta;
    _stateVersion++; // Signal position change

    if (CurrentTrack != null && _currentPosition >= CurrentTrack.Duration)
    {
        await PlayNextTrackAsync();
    }
}
}
