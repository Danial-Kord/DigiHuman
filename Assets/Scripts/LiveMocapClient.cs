using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Receives Backend /ws/live_mocap and drives <see cref="FrameReader"/> each frame.
/// Uses System.Net.WebSockets (e.g. ws://127.0.0.1:5000/ws/live_mocap).
/// </summary>
public class LiveMocapClient : MonoBehaviour
{
    private const string StreamEndSentinel = "__live_mocap_stream_end__";

    [SerializeField] private FrameReader frameReader;
    [SerializeField] private string webSocketUrl = "ws://127.0.0.1:5000/ws/live_mocap";
    [SerializeField] private int cameraId;

    [Header("Optional UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;

    private readonly ConcurrentQueue<string> _incoming = new ConcurrentQueue<string>();
    private CancellationTokenSource _cts;
    private volatile bool _receiveLoopActive;
    private long _sessionId;

    [Serializable]
    private struct LiveWsCommand
    {
        public string cmd;
        public int camera_id;
    }

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartLiveStream);
        if (stopButton != null)
            stopButton.onClick.AddListener(StopLiveStream);
    }

    private void OnDestroy()
    {
        StopLiveStream();
        if (startButton != null)
            startButton.onClick.RemoveListener(StartLiveStream);
        if (stopButton != null)
            stopButton.onClick.RemoveListener(StopLiveStream);
    }

    public void StartLiveStream()
    {
        if (frameReader == null)
        {
            Debug.LogError("LiveMocapClient: assign FrameReader.");
            return;
        }

        StopLiveStream();
        frameReader.SetLiveStreamMode(true);

        _cts = new CancellationTokenSource();
        _receiveLoopActive = true;
        long sid = Interlocked.Increment(ref _sessionId);
        _ = RunWebSocketAsync(_cts.Token, sid);
    }

    public void StopLiveStream()
    {
        Interlocked.Increment(ref _sessionId);
        _receiveLoopActive = false;
        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        _cts?.Dispose();
        _cts = null;

        if (frameReader != null)
            frameReader.SetLiveStreamMode(false);
    }

    private async Task RunWebSocketAsync(CancellationToken ct, long sid)
    {
        ClientWebSocket ws = null;
        try
        {
            ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(webSocketUrl), ct).ConfigureAwait(false);

            var start = new LiveWsCommand { cmd = "start", camera_id = cameraId };
            var startBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(start));
            await ws.SendAsync(new ArraySegment<byte>(startBytes), WebSocketMessageType.Text, true, ct)
                .ConfigureAwait(false);

            var buffer = new byte[1024 * 512];
            while (!ct.IsCancellationRequested && _receiveLoopActive && ws.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                        return;
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                var text = Encoding.UTF8.GetString(ms.ToArray());
                if (!string.IsNullOrEmpty(text))
                    _incoming.Enqueue(text);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Debug.LogWarning("Live mocap WebSocket: " + e.Message);
        }
        finally
        {
            _receiveLoopActive = false;
            try
            {
                if (ws != null && ws.State == WebSocketState.Open)
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None)
                        .ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }

            ws?.Dispose();
            if (sid == Volatile.Read(ref _sessionId))
                _incoming.Enqueue(StreamEndSentinel);
        }
    }

    private void LateUpdate()
    {
        if (frameReader == null)
            return;

        string lastPayload = null;
        var streamEnded = false;
        while (_incoming.TryDequeue(out var chunk))
        {
            if (chunk == StreamEndSentinel)
                streamEnded = true;
            else
                lastPayload = chunk;
        }

        if (!string.IsNullOrEmpty(lastPayload))
            frameReader.ApplyLiveMocapFrameJson(lastPayload);

        if (streamEnded)
            frameReader.SetLiveStreamMode(false);
    }
}
