using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Concept.Helpers;
using NUnit.Framework;
using UnityEngine;


namespace Twinny.System
{

    public class DynamicMeshSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        public bool spawnOnAwake;
        [Tooltip("Milliseconds")]
        public int spawnInterval = 100;
        public int chunkSize = 50;
        [Space]
        public List<Renderer> renderers;

        private void Awake()
        {
                _ = SpawnAsync(false, 0);

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        { 
            if (spawnOnAwake) 
            _ = SpawnAsync(true, spawnInterval); 
        }


        // Update is called once per frame
        void Update()
        {

        }

        public ProgressTask<bool> SpawnAsync(bool active, int delay = 100, int chunkSize = default, CancellationToken cancellationToken = default)
        {
            var progressTaskSource = new TaskCompletionSource<bool>();
            var progressTask = new ProgressTask<bool>(progressTaskSource.Task);

            if(chunkSize == default) chunkSize = this.chunkSize;

            // Roda a task em background (exemplo simplificado)
            RunSpawnAsync(active,progressTask, progressTaskSource, delay, chunkSize, cancellationToken);

            return progressTask;
        }


        private async void RunSpawnAsync(bool active, ProgressTask<bool> progressTask, TaskCompletionSource<bool> tcs, int delay, int chunkSize, CancellationToken cancel)
        {
            if (renderers == null || renderers.Count == 0)
            {
                tcs.SetResult(false);
                return;
            }

            int total = renderers.Count;

            for (int i = 0; i < total; i += chunkSize)
            {
                if (cancel.IsCancellationRequested)
                {
                    tcs.SetCanceled();
                    return;
                }

                for (int j = i; j < i + chunkSize && j < total; j++)
                {
                    renderers[j]?.gameObject.SetActive(active);
                }

                float progress = (i + chunkSize) / (float)total;
                progressTask.ReportProgress(Mathf.Min(progress, 1f));

                // pequeno delay após cada chunk
                await Task.Delay(delay, cancel);
            }

            tcs.SetResult(true);
        }



        private async void RunSpawnAsync2(bool active, ProgressTask<bool> progressTask, TaskCompletionSource<bool> tcs, int delay, CancellationToken cancel)
        {
            if (renderers == null || renderers.Count == 0)
            {
                tcs.SetResult(false);
                return;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                if (cancel.IsCancellationRequested)
                {
                    tcs.SetCanceled();
                    return;
                }

                renderers[i].gameObject.SetActive(active);
                await Task.Delay(delay);

                float progress = (i + 1f) / renderers.Count;
                progressTask.ReportProgress(progress);
            }

            tcs.SetResult(true);
        }



    }

}