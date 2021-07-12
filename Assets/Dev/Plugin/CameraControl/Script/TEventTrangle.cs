using System.Collections;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.Events;
using System;
using TriggerCondition = TMesh.TEvent.TriggerCondition;

namespace TMesh
{
    [Serializable]
    public class TEventTrangle : TTrangle
    {
        public List<TEvent> Events = new List<TEvent>();

        public List<TEventTrangle> Childs = new List<TEventTrangle>();


        private TEventTrangle _parent;
        private Transform _lastParentTrans = null;
        public TEventTrangle parent
        {
            get {
                if (!transform.parent)
                    return null;

                if (_lastParentTrans != transform.parent)
                { 
                    _parent = transform.parent.GetComponent<TEventTrangle>();
                    _lastParentTrans = transform.parent;
                }

                return _parent; 
            }
            set { SetParent(value); }
        }
        public void SetParent(TEventTrangle parent)
        {
            if (parent == null)
            {
                _parent = null;
                return;
            }
            parent.AddChild(this);
        }
        public void AddChild(TEventTrangle obj)
        {
            if (!Childs.Contains(obj))
            {
                Childs.Add(obj);

                obj.parent = this;

                if (this.parent == this)
                {
                    this.parent = null;
                }
            }
        }
        public TEventTrangle GetChild(int index)
        {
            return Childs[index];
        }
        public bool AnyRelationship(TEventTrangle trangle)
        {
            if (trangle == null)
                return false;

            if (this.GetInstanceID() == trangle.GetInstanceID())
                return true;

            if (this.parent && this.parent.GetInstanceID() == trangle.GetInstanceID())
                return true;

            if (trangle.parent && trangle.parent.GetInstanceID() == this.GetInstanceID())
                return true;

            return false;
        }

        public override void Tick()
        {
            if (parent)
            {
                parent.Tick();
                return;
            }
            foreach (var tevent in Events)
            {
                tevent.Tick();
            }
        }
        public override void OnEnterTrangle()
        {
            if (parent)
            {
                parent.OnEnterTrangle();
                return;
            }
            foreach (var tevent in Events)
            {
                tevent.OnEnter();
            }
        }
        public override void OnExitTrangle() 
        {
            if (parent)
            {
                parent.OnExitTrangle();
                return;
            }
            foreach (var tevent in Events)
            {
                tevent.OnExit();
            }
        }
    }


    [Serializable]
    public class OnTargetEvent : UnityEvent
    {
    }

    [Serializable]
    public enum TriggerEventType
    {
        Timeline,
        Animation,
        Do,
    }

    [Serializable]
    public class TEvent
    {
        public TriggerEventType eventType = TriggerEventType.Timeline;
        public TEvent(TriggerEventType eventType)
        {
            this.eventType = eventType;
        }
        public PlayableDirector playableDirector;
        public Animation animation;
        public OnTargetEvent onTrigger = new OnTargetEvent();

        public TTrangle trangle;

        public bool isDone = false;

        public int canTriggerTimes = 1;
        public enum TriggerCondition : int
        {
            Enter,
            Exit,
            WaitASecond,
            UpdateOnceEnter,
        }

        public TriggerCondition condition = TriggerCondition.Enter;

        public string error = string.Empty;

        public int enterCount = 0;


        public bool useUnscaledTime = true;

        public float waitingSceond = 0f;

        private float waitingCounter = 0f;

        public virtual bool Tick()
        {
            if (isDone)
                return false;

            if (!trangle)
                return false;

            if (!string.IsNullOrEmpty(error))
                return false;

            if (condition == TriggerCondition.WaitASecond)
            {
                if (waitingSceond <= 0f)
                {
                    return true;
                }

                waitingCounter += useUnscaledTime ? Time.unscaledTime : Time.deltaTime;

                if (waitingCounter >= waitingSceond)
                {
                    OnCondition(TriggerCondition.WaitASecond);
                    return true;
                }
            }
            else if (condition == TriggerCondition.UpdateOnceEnter && enterCount > 0)
            {
                OnCondition(TriggerCondition.WaitASecond);
            }

            return false;
        }

        private bool OnCondition(TriggerCondition condition)
        {
            if (isDone)
                return false;

            if (this.condition != condition)
                return false;

            Done();
            return true;
        }

        public virtual bool OnEnter()
        {
            enterCount++;
            return OnCondition(TriggerCondition.Enter);
        }
        public virtual bool OnExit()
        {
            return OnCondition(TriggerCondition.Exit);
        }

        public virtual void Done()
        {
            if (eventType == TriggerEventType.Timeline)
            {
                playableDirector.Play();
            }
            else if (eventType == TriggerEventType.Animation)
            {
                animation.Play();
            }
            else if (eventType == TriggerEventType.Do)
            {
                onTrigger.Invoke();
            }

            if (condition == TriggerCondition.UpdateOnceEnter)
            {
                return;
            }

            isDone = enterCount >= canTriggerTimes;

            if (canTriggerTimes < 0)
            {
                isDone = false;
                canTriggerTimes = -1;
            }
        }

        public void Dispose()
        {
        }
    }

}
