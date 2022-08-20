//-- https://github.com/adainrivers/randomencounters/blob/2e0f4357cba5b32bfe9bcdc6c5324ef4d78d3677/src/Components/TaskRunner.cs

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RPGMods.Hooks;
using Unity.Entities;

namespace RPGMods.Utils
{
    public class TaskRunner
    {
        private static readonly ConcurrentQueue<RisingTask> PriorityTaskQueue = new();
        private static readonly ConcurrentQueue<RisingTask> TaskQueue = new();
        private static readonly SizedDictionaryAsync<Guid, RisingTaskResult> TaskResults = new(100);

        public static void Initialize()
        {
            SCSHook.OnUpdate += Update;
        }

        public static Guid Start(Func<World, object> func, bool HighPriority = false, bool runNow = true, bool getResult = false, TimeSpan startAfter = default)
        {
            var risingTask = new RisingTask
            {
                ResultFunction = func,
                RunNow = runNow,
                GetResult = getResult,
                StartAfter = DateTime.UtcNow.Add(startAfter),
                TaskId = Guid.NewGuid()
            };
            if (HighPriority) PriorityTaskQueue.Enqueue(risingTask);
            else TaskQueue.Enqueue(risingTask);
            return risingTask.TaskId;
        }

        public static object GetResult(Guid taskId)
        {
            return TaskResults.TryGetValue(taskId, out var result) ? result : null;
        }

        private static void Update(World world)
        {
            if (PriorityTaskQueue.Count > 0)
            {
                for(int i = 0; i < PriorityTaskQueue.Count; i++)
                {
                    if (!PriorityTaskQueue.TryDequeue(out var prioritytask))
                    {
                        continue;
                    }

                    object priorityresult;
                    try
                    {
                        priorityresult = prioritytask.ResultFunction.Invoke(world);
                    }
                    catch
                    {
                        if (prioritytask.GetResult) TaskResults.Add(prioritytask.TaskId, new RisingTaskResult { Result = "Error" });
                        continue;
                    }

                    if (prioritytask.GetResult) TaskResults.Add(prioritytask.TaskId, new RisingTaskResult { Result = priorityresult });
                }
            }

            if (!TaskQueue.TryDequeue(out var task))
            {
                return;
            }

            if (!task.RunNow)
            {
                if (task.StartAfter > DateTime.UtcNow)
                {
                    TaskQueue.Enqueue(task);
                    return;
                }
            }

            object result;
            try
            {
                result = task.ResultFunction.Invoke(world);
            }
            catch
            {
                if (task.GetResult) TaskResults.Add(task.TaskId, new RisingTaskResult { Result = "Error" });
                return;
            }

            if (task.GetResult) TaskResults.Add(task.TaskId, new RisingTaskResult { Result = result });
        }

        public static void Destroy()
        {
            SCSHook.OnUpdate -= Update;
            TaskQueue.Clear();
            TaskResults.Clear();
        }

        private class RisingTask
        {
            public Guid TaskId { get; set; } = Guid.NewGuid();
            public bool RunNow { get; set; }
            public bool GetResult { get; set; }
            public DateTime StartAfter { get; set; }
            public Func<World, object> ResultFunction { get; set; }
        }
    }

    public class RisingTaskResult
    {
        public object Result { get; set; }
        public Exception Exception { get; set; }
    }
}