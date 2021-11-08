using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace GoodCat.Fsm.FsmHelpers
{
    public class StatesCollection
    {
        public IReadOnlyList<State> List => _list;
        private readonly List<State> _list = new List<State>();
        [NotNull] public State StateStartDefault { get; private set; }

        public readonly TransitionsHelper Transitions;

        /// <summary>
        /// Null if this _fsm.StatesCollection
        /// </summary>
        [CanBeNull] internal readonly StateHierarchical StateHierarchicalOwner;

        private readonly FSM _fsm;
        private bool IsThisStatesInContainer => StateHierarchicalOwner != null;

        public StatesCollection(FSM fsm, [CanBeNull] StateHierarchical stateHierarchicalOwner)
        {
            _fsm = fsm;
            StateHierarchicalOwner = stateHierarchicalOwner;
            Transitions = new TransitionsHelper(this);
        }

        #region Get
        public StatesCollection GetParentStatesCollection()
        {
            Assert.IsNotNull(StateHierarchicalOwner);
            var parent = StateHierarchicalOwner.GetParent();
            return parent == null ? _fsm.StatesCollection : parent.StatesCollection;
        }

        public StateHierarchical GetStateHierarchical(string name)
        {
            var parent = GetState<StateHierarchical>(name);
            return parent;
        }

        public StateSimple GetState(string idStateSimple) => GetState<StateSimple>(idStateSimple);

        public T GetState<T>(string name = "") where T : State
        {
            var type = typeof(T);
            return (T)GetState(type, name);
        }

        public State GetState(Type type, string name = "")
        {
            var errorNameString = name == "" ? "" : $" Name: {name}";
            var error = $"GetState - cant find {type.Name} {errorNameString}";
            State result = null;
            var isStateExist = TryGetState(type, out result, name);
            Assert.IsTrue(isStateExist, error);
            return result;
        }

        public bool TryGetState(out State result, string name = "")
            => TryGetState<StateSimple>(out result, name);
        
        public bool TryGetState<T>(out State result, string name = "")
            => TryGetState(typeof(T), out result, name);
        
        public bool TryGetState(Type type, out State result, string name = "")
        {
            result = List.Where(state => state.GetType() == type)
                .SingleOrDefault(state => state.Name == name);
            return result != null;
        }
        #endregion

        #region Add
        public StateHierarchical AddHierarchical(string id)
        {
            var container = new StateHierarchical(_fsm, id);
            Add(container);
            return container;
        }

        public StatesCollection Add([NotNull] string name,
            [CanBeNull] Action onEnableAction,
            [CanBeNull] Action onUpdateAction,
            [CanBeNull] Action onDisableAction, bool isStartState = false)
        {
            var stateSimple = State.Simple(name, onEnableAction, onUpdateAction, onDisableAction);
            return Add(stateSimple, isStartState);
        }

        public StatesCollection Add(State state, bool isStartState = false)
        {
            CheckWriteMode();
            CheckIdUniq(state, state.Name);
            _list.Add(state);

            if (IsThisStatesInContainer)
                state.SetParent(StateHierarchicalOwner);

            if (isStartState)
                SetStartState(state);
            return this;
        }
        #endregion

        #region SetStartState
        public StatesCollection SetStartState<T>(string idStateSimple)
            where T : State
        {
            var state = GetState<T>(idStateSimple);
            return SetStartState(state);
        }

        public StatesCollection SetStartState(string idStateSimple)
        {
            var stateSimple = GetState(idStateSimple);
            return SetStartState(stateSimple);
        }

        public StatesCollection SetStartState(State state)
        {
            Assert.IsTrue(List.Contains(state), $"States does not contain {state}!");
            StateStartDefault = state;
            return this;
        }
        #endregion

        #region Remove
        public StatesCollection Remove(string idStateSimple) => Remove<StateSimple>(idStateSimple);

        public StatesCollection Remove<T>(string name = "") where T : State
        {
            var state = GetState<T>(name);
            return Remove(state);
        }

        public StatesCollection Remove(State state)
        {
            Assert.IsTrue(List.Contains(state));
            CheckWriteMode();
            Assert.IsTrue(state != StateStartDefault,
                "Can't remove startState, assign another startState first" +
                "(SetStartState(state) or Add(state, true))");
            _list.Remove(state);
            return this;
        }
        #endregion

        internal bool TryGetStateRunning(out State result)
        {
            foreach (var state in List)
            {
                if (state.IsEnable)
                {
                    result = state;
                    return true;
                }

                if (state is StateHierarchical container && container.StatesCollection.TryGetStateRunning(out result))
                    return true;
            }

            result = null;
            return false;
        }

        #region Check
        internal void CheckStartStatesRecursively()
        {
            var owner = IsThisStatesInContainer ? StateHierarchicalOwner.Name : "FSM Root";
            Assert.IsNotNull(StateStartDefault,
                $"Not set start state in {owner}!");

            foreach (var state in _list)
            {
                if (state is StateHierarchical stateHierarchical)
                {
                    stateHierarchical.StatesCollection.CheckStartStatesRecursively();
                }
            }
        }

        internal void CheckTransitionsExistRecursively()
        {
            foreach (var state in _list)
            {
                if (state is StateHierarchical stateHierarchical)
                {
                    stateHierarchical.StatesCollection.CheckTransitionsExistRecursively();
                }
             
                Assert.IsTrue(state.TransitionsList.Count > 0, $"State {state} has no transitions!");
   
            }
        }

        private void CheckIdUniq<T>(T state, string id)
            where T : State
        {
            var type = typeof(T);
            CheckIdUniq(type, id);
        }

        private void CheckIdUniq(Type stateType, string id)
        {
            Assert.IsFalse(_list.Any(state => state.GetType() == stateType && state.Name == id),
                $"State (type: {stateType.GetType().Name}, name: {id}) already exists!");
        }

        private void CheckWriteMode() => Assert.IsTrue(_fsm.Mode == FSM.ModeEnum.Init, "Can't modify running FSM!");
        #endregion
    }
}