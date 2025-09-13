// DamageAggregator.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Lightweight, non-MonoBehaviour label aggregator. Start() spins up a background task that
/// periodically consumes enqueued DamageEvents, clusters them and pushes AggregatedLabel results
/// into an internal result queue. Call DrainResults() from the main thread (Game_Manager) to
/// retrieve and dispatch labels to clients.
/// </summary>
public class ServerLabelAgregator : IDisposable
{
    // Public tuning
    public readonly float ClusterRadius;
    public readonly float ProcessIntervalSeconds;
    public readonly float MinLabelThreshold;

    // Thread-safe queues
    private readonly ConcurrentQueue<DamageEvent> inputQueue = new();
    private readonly ConcurrentQueue<AggregatedLabel> resultsQueue = new();

    // background worker
    private CancellationTokenSource cts;
    private Task workerTask;

    public ServerLabelAgregator(float clusterRadius = 2f, float processIntervalSeconds = 0.15f, float minLabelThreshold = 1f)
    {
        ClusterRadius = Math.Max(0.0001f, clusterRadius);
        ProcessIntervalSeconds = Math.Max(0.001f, processIntervalSeconds);
        MinLabelThreshold = Math.Max(0f, minLabelThreshold);
    }

    // Call once to start background processing
    public void Start()
    {
        if (cts != null) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        workerTask = Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(ProcessIntervalSeconds), token).ConfigureAwait(false);
                    // Drain input queue to local list (fast)
                    if (!inputQueue.IsEmpty)
                    {
                        var local = new List<DamageEvent>();
                        while (inputQueue.TryDequeue(out var ev))
                            local.Add(ev);

                        if (local.Count > 0)
                            ProcessAndAggregate(local);
                    }
                }
            }
            catch (OperationCanceledException) { /* expected on stop */ }
        }, token);
    }

    // Stop background processing and wait for worker to end
    public void Stop()
    {
        if (cts == null) return;
        cts.Cancel();
        try { workerTask?.Wait(500); } catch { /* ignore */ }
        cts.Dispose();
        cts = null;
        workerTask = null;
    }

    public void Dispose() => Stop();

    // Enqueue from main thread/server logic (cheap and thread-safe)
    public void EnqueueDamage(in DamageEvent ev)
    {
        if (ev.amount <= 0f) return;
        inputQueue.Enqueue(ev);
    }

    // Called from main thread to get all aggregated labels produced so far.
    public List<AggregatedLabel> DrainResults()
    {
        var outList = new List<AggregatedLabel>();
        while (resultsQueue.TryDequeue(out var lab))
            outList.Add(lab);
        return outList;
    }

    // --- internal processing (runs off main thread) ---
    private void ProcessAndAggregate(List<DamageEvent> events)
    {
        // spatial hash keying: combine color + integer grid coords
        float r = ClusterRadius;
        float invR = 1f / r;

        var buckets = new Dictionary<long, BucketData>();

        foreach (var ev in events)
        {
            int hx = (int)Math.Floor(ev.worldX * invR);
            int hz = (int)Math.Floor(ev.worldZ * invR);
            int colorId = (int)ev.color;

            long key = ((long)colorId << 48) | ((long)(uint)hx << 24) | (uint)hz;

            if (!buckets.TryGetValue(key, out var bd))
            {
                bd = new BucketData(ev.color);
                buckets[key] = bd;
            }

            bd.sumAmount += ev.amount;
            bd.weightedX += ev.worldX * ev.amount;
            bd.weightedZ += ev.worldZ * ev.amount;

            if (ev.sourcePlayerId >= 0)
            {
                if (!bd.playerDamageMap.TryGetValue(ev.sourcePlayerId, out var pd)) pd = 0f;
                pd += ev.amount;
                bd.playerDamageMap[ev.sourcePlayerId] = pd;
                if (pd > bd.topPlayerDamage)
                {
                    bd.topPlayerDamage = pd;
                    bd.representativePlayerId = ev.sourcePlayerId;
                    bd.playerPosX = ev.playerWorldX;
                    bd.playerPosY = ev.playerWorldY;
                    bd.playerPosZ = ev.playerWorldZ;
                }
            }
        }

        // Create aggregated labels for buckets above threshold
        foreach (var kv in buckets)
        {
            var bd = kv.Value;
            if (bd.sumAmount < MinLabelThreshold) continue;

            float cx = bd.weightedX / bd.sumAmount;
            float cz = bd.weightedZ / bd.sumAmount;

            var label = new AggregatedLabel
            {
                color = bd.color,
                totalAmount = bd.sumAmount,
                clusterX = cx,
                clusterY = 0f,
                clusterZ = cz,
                representativePlayerId = bd.representativePlayerId,
                playerX = bd.playerPosX,
                playerY = bd.playerPosY,
                playerZ = bd.playerPosZ
            };

            resultsQueue.Enqueue(label);
        }
    }

    // Helper bucket class (reference type so no parameterless struct ctors)
    private class BucketData
    {
        public CellColor color;
        public float sumAmount;
        public float weightedX;
        public float weightedZ;

        public int representativePlayerId;
        public float playerPosX, playerPosY, playerPosZ;

        // track player damage contribution in this bucket
        public Dictionary<int, float> playerDamageMap;
        public float topPlayerDamage;

        public BucketData(CellColor c)
        {
            color = c;
            sumAmount = 0f;
            weightedX = weightedZ = 0f;
            representativePlayerId = -1;
            playerPosX = playerPosY = playerPosZ = 0f;
            playerDamageMap = new Dictionary<int, float>();
            topPlayerDamage = 0f;
        }
    }
}

// A small POD struct for enqueuing events (no custom parameterless ctor)
public struct DamageEvent
{
    public int sourcePlayerId;   // -1 if none
    public CellColor color;
    public float amount;
    public float worldX;
    public float worldY;
    public float worldZ;
    public float playerWorldX;
    public float playerWorldY;
    public float playerWorldZ;
}

// Aggregated label produced by background processing
public class AggregatedLabel
{
    public CellColor color;
    public float totalAmount;
    public float clusterX, clusterY, clusterZ;
    public int representativePlayerId;
    public float playerX, playerY, playerZ;
}

