using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GoodCat.Fsm
{
    public abstract class State : IDisposable
    {
        public static StateSimple Simple([NotNull] string name,
            [CanBeNull] Action onEnableAction,
            [CanBeNull] Action onUpdateAction,
            [CanBeNull] Action onDisableAction) =>
            new StateSimple(name, onEnableAction, onUpdateAction, onDisableAction);

        public readonly string Name;
        internal bool IsEnable { get; private set; }
        internal IReadOnlyList<Transition> TransitionsList => _transitionsList;
        private readonly List<Transition> _transitionsList = new List<Transition>();

        [CanBeNull] private StateHierarchical _parent;

        private readonly string _nameForToString;

        protected State(string name = "")
        {
            Name = name;
            _nameForToString = string.IsNullOrEmpty(name) ? GetType().Name : name;
        }

        internal void StartState() => Enable();

        internal void AddTransition(Transition transition) => _transitionsList.Add(transition);

        internal void RemoveTransition(Transition transition) => _transitionsList.Remove(transition);

        internal void SetParent(StateHierarchical stateHierarchical) => _parent = stateHierarchical;

        [CanBeNull]
        internal StateHierarchical GetParent() => _parent;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if was transition</returns>
        internal bool UpdateState()
        {
            if (_parent != null)
            {
                var isStateParentUpdated = _parent.UpdateState();
                if (isStateParentUpdated) return true;
            }

            if (DoTransitionIfNeed())
                return true;
            
            return OnUpdate();
        }

        protected abstract void OnEnable();

        /// <summary>
        ///  
        /// </summary>
        /// <returns>True if was transition
        /// Check transition: if(DoTransitionIfNeed()) return true;</returns>
        protected abstract bool OnUpdate();

        protected abstract void OnDisable();

        protected virtual void OnDispose() {}
        
        private void Enable()
        {
            OnEnable();
            if (this is StateHierarchical == false)
                IsEnable = true;
        }

        internal void Disable()
        {
            OnDisable();
            IsEnable = false;
        }

        /// <summary>
        /// You can use it in your custom OnUpdate()
        /// if(DoTransitionIfNeed()) return true;
        /// </summary>
        /// <returns>True if was transition</returns>
        protected bool DoTransitionIfNeed()
        {
            if (IsNeedTransition(out var result) == false)
                return false;

            DoTransition(result);
            return true;
        }

        private void DoTransition(Transition transition)
        {
            Disable();
            transition.Target.Enable();
        }

        private bool IsNeedTransition(out Transition result)
        {
            for (var i = 0; i < _transitionsList.Count; i++)
            {
                var transition = _transitionsList[i];
                if (transition.Condition.IsTrue() == false) continue;
                result = transition;
                return true;
            }

            result = null;
            return false;
        }

        public override string ToString() => _nameForToString;

        void IDisposable.Dispose() => OnDispose();
    }
}