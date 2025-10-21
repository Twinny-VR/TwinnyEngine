using System.Threading.Tasks;
using System;
using UnityEngine.Events;
using UnityEngine;

namespace Twinny.GamePlay
{

    [Serializable]
    public class BreakPoint
    {
        public enum BreakPointType
        {
            PAUSE,
            AWAIT,
            SIGNAL
        }

        public enum BreakPointState
        {
            IDLE,
            PAUSED
        }

        public int position;
        [SerializeField]
        private BreakPointType m_breakPointType;
        public BreakPointType breakPointType
        {
            get => m_breakPointType;
            set
            {
                m_breakPointType = value;
                m_awake = value == BreakPointType.AWAIT;
            }
        }

        public BreakPointState state { get; private set; }


        [HideInInspector]
        public bool m_awake;

        [Tooltip("Milliseconds")]
        public int awaitDelay = 1000;

        public UnityEvent OnBreakEvent = new UnityEvent();
        public UnityEvent OnResumeEvent = new UnityEvent();

        public async Task<bool> Break()
        {

            if(breakPointType == BreakPointType.SIGNAL)
            {
                OnBreakEvent?.Invoke();
                Resume();
                return true;
            }

            state = BreakPointState.PAUSED;

            if(breakPointType == BreakPointType.PAUSE && awaitDelay > 0) 
                await Task.Delay(awaitDelay);


            OnBreakEvent?.Invoke();

            if (breakPointType == BreakPointType.AWAIT)
            {
                await Task.Delay(awaitDelay);
                Resume();
                return true;
            }
            return false;
        }

        public void Resume()
        {
            state = BreakPointState.IDLE;
            OnResumeEvent?.Invoke();
        }

    }
}