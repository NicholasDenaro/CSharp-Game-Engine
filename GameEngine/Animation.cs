﻿using GameEngine.Interfaces;
using System.Collections.Generic;

namespace GameEngine
{
    public class AnimationManager
    {
        private Dictionary<string, Animation> animations = new Dictionary<string, Animation>();

        private static AnimationManager singleton;
        public static AnimationManager Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new AnimationManager();
                }

                return singleton;
            }
        }

        public void Add(Animation ani)
        {
            animations.Add(ani.Name, ani);
        }

        public Animation this[string name]
        {
            get => animations[name].CreateNew();
        }
    }

    public class AnimationChain : Stack<Animation>
    {
        public AnimationChain(params Animation[] anis)
        {
            foreach (Animation ani in anis)
            {
                Push(ani);
            }
        }

        public bool Tick(IDescription description)
        {
            return Peek().Tick(description);
        }
    }

    public class Animation
    {
        public delegate bool TriggerDelegate(IDescription description);
        public delegate void TickDelegate(IDescription description);
        public delegate void FinalDelegate(IDescription description);

        public string Name { get; private set; }
        public int Duration { get; private set; }

        private int time;

        private bool interruptable;

        private TriggerDelegate trigger;

        private TickDelegate onTick;

        private FinalDelegate onFinal;
        public Animation(string name, int duration, TriggerDelegate trigger, TickDelegate tick, FinalDelegate final)
        {
            Name = name;
            Duration = duration;
            AnimationManager.Instance.Add(this);
            this.trigger = trigger;
            onTick = tick;
            onFinal = final;
        }

        private Animation()
        {
        }

        public Animation CreateNew()
        {
            Animation ani = new Animation();
            ani.Name = Name;
            ani.Duration = Duration;
            ani.time = Duration;
            ani.trigger = trigger;
            ani.onTick = onTick;
            ani.onFinal = onFinal;
            return ani;
        }

        public Animation Trigger(TriggerDelegate trigger)
        {
            this.trigger = trigger;
            return this;
        }

        public Animation MakeInterruptable()
        {
            this.interruptable = true;
            return this;
        }

        public bool IsInterruptable()
        {
            return this.interruptable;
        }

        public int TicksLeft()
        {
            return time;
        }

        public bool Tick(IDescription description)
        {
            if (time == -1)
            {
                if (trigger(description))
                {
                    return true;
                }

                onTick?.Invoke(description);
            }
            else if (IsStarted() || (trigger?.Invoke(description) ?? true))
            {
                if (time > 1)
                {
                    onTick?.Invoke(description);
                }
                else if (time > 0)
                {
                    onFinal?.Invoke(description);
                    return true;
                }

                if (time > 0)
                {
                    time--;
                }
            }

            return false;
        }

        public bool IsDone()
        {
            return time == 0;
        }

        public bool IsStarted()
        {
            return time != Duration;
        }
    }
}