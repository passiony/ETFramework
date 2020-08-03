using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ETModel
{
    public interface ITimer
    {
        void Run();
    }

    [ObjectSystem]
    public class OnceWaitTimerAwakeSystem : AwakeSystem<OnceWaitTimer, ETTaskCompletionSource>
    {
        public override void Awake(OnceWaitTimer self, ETTaskCompletionSource callback)
        {
            self.Callback = callback;
        }
    }

    [ObjectSystem]
    public class WaitUntilAwakeSystem : AwakeSystem<WaitUntil, ETTaskCompletionSource>
    {
        public override void Awake(WaitUntil self, ETTaskCompletionSource callback)
        {
            self.Callback = callback;
        }
    }

    public class OnceWaitTimer : Entity, ITimer
    {
        public ETTaskCompletionSource Callback { get; set; }

        public void Run()
        {
            ETTaskCompletionSource tcs = this.Callback;
            this.GetParent<TimerComponent>().Remove(this.Id);
            tcs.SetResult();
        }
    }

    public class WaitUntil : Entity, ITimer
    {
        public ETTaskCompletionSource Callback { get; set; }

        public void Run()
        {
            var tcs = this.Callback;
            tcs.SetResult();
        }
    }

    [ObjectSystem]
    public class OnceTimerAwakeSystem : AwakeSystem<OnceTimer, Action>
    {
        public override void Awake(OnceTimer self, Action callback)
        {
            self.Callback = callback;
        }
    }

    public class OnceTimer : Entity, ITimer
    {
        public Action Callback { get; set; }

        public void Run()
        {
            try
            {
                this.Callback.Invoke();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    [ObjectSystem]
    public class RepeatedTimerAwakeSystem : AwakeSystem<RepeatedTimer, long, int, Action>
    {
        public override void Awake(RepeatedTimer self, long repeatedTime, int repeatNum, Action callback)
        {
            self.Awake(repeatedTime, repeatNum, callback);
        }
    }

    public class RepeatedTimer : Entity, ITimer
    {
        public void Awake(long repeatedTime, int repeatNum, Action callback)
        {
            this.StartTime = TimeHelper.Now();
            this.RepeatedTime = repeatedTime;
            this.Callback = callback;
            this.Count = 0;
            this.targetCount = repeatNum;
        }

        private long StartTime { get; set; }

        private long RepeatedTime { get; set; }

        // 下次一是第几次触发
        private int Count { get; set; }

        private int targetCount { get; set; }

        public Action Callback { private get; set; }

        public void Run()
        {
            TimerComponent timerComponent = this.GetParent<TimerComponent>();

            if (this.Count >= targetCount && this.targetCount > 0)
            {
                timerComponent.Remove(this.Id);
                return;
            }

            ++this.Count;
            long tillTime = this.StartTime + this.RepeatedTime * this.Count;
            timerComponent.AddToTimeId(tillTime, this.Id);

            try
            {
                this.Callback.Invoke();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            long id = this.Id;

            if (id == 0)
            {
                Log.Error($"RepeatedTimer可能多次释放了");
                return;
            }

            base.Dispose();

            this.StartTime = 0;
            this.RepeatedTime = 0;
            this.Callback = null;
            this.Count = 0;
        }
    }

    [ObjectSystem]
    public class TimerComponentAwakeSystem : AwakeSystem<TimerComponent>
    {
        public override void Awake(TimerComponent self)
        {
            TimerComponent.Instance = self;
        }
    }

    [ObjectSystem]
    public class TimerComponentUpdateSystem : UpdateSystem<TimerComponent>
    {
        public override void Update(TimerComponent self)
        {
            self.Update();
            self.UntilUpdate();
        }
    }

    public class TimerComponent : Entity
    {
        public static TimerComponent Instance { get; set; }

        private readonly Dictionary<long, ITimer> timers = new Dictionary<long, ITimer>();
        private readonly Dictionary<Func<bool>, ITimer> untils = new Dictionary<Func<bool>, ITimer>();

        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        public readonly MultiMap<long, long> TimeId = new MultiMap<long, long>();

        private readonly Queue<long> timeOutTime = new Queue<long>();

        private readonly Queue<long> timeOutTimerIds = new Queue<long>();

        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime;

        public void Update()
        {
            if (this.TimeId.Count == 0)
            {
                return;
            }

            long timeNow = TimeHelper.Now();

            if (timeNow < this.minTime)
            {
                return;
            }

            foreach (KeyValuePair<long, List<long>> kv in this.TimeId.GetDictionary())
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    minTime = k;
                    break;
                }
                this.timeOutTime.Enqueue(k);
            }

            while (this.timeOutTime.Count > 0)
            {
                long time = this.timeOutTime.Dequeue();
                foreach (long timerId in this.TimeId[time])
                {
                    this.timeOutTimerIds.Enqueue(timerId);
                }
                this.TimeId.Remove(time);
            }

            while (this.timeOutTimerIds.Count > 0)
            {
                long timerId = this.timeOutTimerIds.Dequeue();
                ITimer timer;
                if (!this.timers.TryGetValue(timerId, out timer))
                {
                    continue;
                }

                timer.Run();
            }
        }

        public void UntilUpdate()
        {
            if (untils.Count <= 0) return;
            var fulfilled = untils.Where(until => until.Key()).ToList();
            foreach (var until in fulfilled)
            {
                untils.TryGetValue(until.Key, out ITimer timer);
                timer.Run();
                untils.Remove(until.Key);
            }
        }

        public ETTask WaitTillAsync(long tillTime, ETCancellationToken cancellationToken)
        {
            if (TimeHelper.Now() > tillTime)
            {
                return ETTask.CompletedTask;
            }
            ETTaskCompletionSource tcs = new ETTaskCompletionSource();
            OnceWaitTimer timer = EntityFactory.CreateWithParent<OnceWaitTimer, ETTaskCompletionSource>(this, tcs);
            this.timers[timer.Id] = timer;
            AddToTimeId(tillTime, timer.Id);
            cancellationToken.Register(() => { this.Remove(timer.Id); });
            return tcs.Task;
        }

        public ETTask WaitTillAsync(long tillTime)
        {
            if (TimeHelper.Now() > tillTime)
            {
                return ETTask.CompletedTask;
            }
            ETTaskCompletionSource tcs = new ETTaskCompletionSource();
            OnceWaitTimer timer = EntityFactory.CreateWithParent<OnceWaitTimer, ETTaskCompletionSource>(this, tcs);
            this.timers[timer.Id] = timer;
            AddToTimeId(tillTime, timer.Id);
            return tcs.Task;
        }

        public ETTask WaitAsync(long time, ETCancellationToken cancellationToken)
        {
            long tillTime = TimeHelper.Now() + time;

            if (TimeHelper.Now() > tillTime)
            {
                return ETTask.CompletedTask;
            }

            ETTaskCompletionSource tcs = new ETTaskCompletionSource();
            OnceWaitTimer timer = EntityFactory.CreateWithParent<OnceWaitTimer, ETTaskCompletionSource>(this, tcs);
            this.timers[timer.Id] = timer;
            AddToTimeId(tillTime, timer.Id);
            cancellationToken.Register(() => { this.Remove(timer.Id); });
            return tcs.Task;
        }

        public ETTask WaitUntilAsync(Func<bool> predicate)
        {
            ETTaskCompletionSource tcs = new ETTaskCompletionSource();
            WaitUntil until = EntityFactory.CreateWithParent<WaitUntil, ETTaskCompletionSource>(this, tcs);
            this.untils.Add(predicate, until);
            return tcs.Task;
        }

        public ETTask WaitAsync(long time)
        {
            long tillTime = TimeHelper.Now() + time;
            ETTaskCompletionSource tcs = new ETTaskCompletionSource();
            OnceWaitTimer timer = EntityFactory.CreateWithParent<OnceWaitTimer, ETTaskCompletionSource>(this, tcs);
            this.timers[timer.Id] = timer;
            AddToTimeId(tillTime, timer.Id);
            return tcs.Task;
        }

        /// <summary>
        /// 创建一个RepeatedTimer
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public long NewRepeatedTimer(long time, int count, Action action)
        {
            if (time < 30)
            {
                throw new Exception($"repeated time < 30");
            }
            long tillTime = TimeHelper.Now() + time;
            RepeatedTimer timer = EntityFactory.CreateWithParent<RepeatedTimer, long, int, Action>(this, time, count, action);
            this.timers[timer.Id] = timer;
            AddToTimeId(tillTime, timer.Id);
            return timer.Id;
        }

        public RepeatedTimer GetRepeatedTimer(long id)
        {
            if (!this.timers.TryGetValue(id, out ITimer timer))
            {
                return null;
            }
            return timer as RepeatedTimer;
        }

        public void Remove(long id)
        {
            if (id == 0)
            {
                return;
            }
            ITimer timer;
            if (!this.timers.TryGetValue(id, out timer))
            {
                return;
            }
            this.timers.Remove(id);

            (timer as IDisposable)?.Dispose();
        }

        public long NewOnceTimer(long time, Action action)
        {
            long tillTime = TimeHelper.Now() + time;
            OnceTimer timer = EntityFactory.CreateWithParent<OnceTimer, Action>(this, action);
            this.timers[timer.Id] = timer;
            AddToTimeId(tillTime, timer.Id);
            return timer.Id;
        }

        public Func<long, long> Throttle(long time, Action action)
        {
            var id = NewOnceTimer(time, action);
            return (inputTime) =>
            {
                foreach (var kv in this.TimeId.GetDictionary())
                {
                    if (kv.Value.Contains(id) && kv.Key < TimeHelper.Now())
                    {
                        Remove(id);
                        id = NewOnceTimer(inputTime, action);
                        return id;
                    }
                }
                id = NewOnceTimer(inputTime, action);
                return id;
            };
        }

        public OnceTimer GetOnceTimer(long id)
        {
            if (!this.timers.TryGetValue(id, out ITimer timer))
            {
                return null;
            }
            return timer as OnceTimer;
        }

        public void AddToTimeId(long tillTime, long id)
        {
            this.TimeId.Add(tillTime, id);
            if (tillTime < this.minTime)
            {
                this.minTime = tillTime;
            }
        }
    }
}