using System;
using GoodCat.Fsm.FsmHelpers;
using UnityEngine.Assertions;

namespace GoodCat.Fsm
{
    public class FSM : IDisposable
    {
        public enum ModeEnum
        {
            Init,
            Launched
        }

        public event Action<State, State> OnStateChange;
        public State StateCurrent { get; private set; }
        public readonly StatesCollection StatesCollection;
        public ModeEnum Mode { get; private set; } = ModeEnum.Init;

        public FSM() => StatesCollection = new StatesCollection(this, null);

        private bool _isFirstUpdate = true;
        
        public virtual void Initialize()
        {
            StatesCollection.CheckStartStatesRecursively();
            StatesCollection.CheckTransitionsExistRecursively();
            StateCurrent = StatesCollection.StateStartDefault;
            Mode = ModeEnum.Launched;
        }

        /// <summary>
        /// True - on change stateCurrent
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            Assert.IsTrue(Mode == ModeEnum.Launched);
            Assert.IsNotNull(StateCurrent);

            if (_isFirstUpdate)
            {
                StateCurrent.StartState();
                _isFirstUpdate = false;
            }
            
            var isStateChanged = StateCurrent.UpdateState();

            if (isStateChanged)
            {
                var isStateRunning = StatesCollection.TryGetStateRunning(out var stateCurrent);
                Assert.IsTrue(isStateRunning);
                var statePrevious = StateCurrent;
                StateCurrent = stateCurrent;
                OnStateChange?.Invoke(statePrevious, StateCurrent);
            }

            return isStateChanged;
        }

        public void Dispose()
        {
            foreach (var state in StatesCollection.List)
            {
                ((IDisposable)state).Dispose();
            }
        }
    }
}