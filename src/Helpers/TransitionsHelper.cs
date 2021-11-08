using System;
using System.Linq;
using GoodCat.Conditions;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace GoodCat.Fsm.FsmHelpers
{
    public class TransitionsHelper
    {
        public readonly StatesCollection StatesCollection;

        private State _from;
        private State _to;

        public TransitionsHelper(StatesCollection statesCollection)
        {
            StatesCollection = statesCollection;
        }

        #region From

        public TransitionsHelper FromThis()
        {
            Assert.IsNotNull(StatesCollection.StateHierarchicalOwner, "Can only be used from Parent");
            _from = StatesCollection.StateHierarchicalOwner;
            return this;
        }

        public TransitionsHelper FromStateHierarchical(string idState) => From<StateHierarchical>(idState);

        public TransitionsHelper From(string idStateSimple) => From<StateSimple>(idStateSimple);

        public TransitionsHelper From<T>(string idState = "")
            where T : State
        {
            _from = StatesCollection.GetState<T>(idState);
            return this;
        }

        public TransitionsHelper From(State state)
        {
            Assert.IsTrue(StatesCollection.List.Contains(state));
            _from = state;
            return this;
        }

        #endregion

        #region To
        public TransitionsHelper ToStateHierarchical(string idState) => To<StateHierarchical>(idState);

        public TransitionsHelper To(string idStateSimple) => To<StateSimple>(idStateSimple);

        public TransitionsHelper To<T>(string idState = "")
            where T : State
        {
            _to = StatesCollection.GetState<T>(idState);
            return this;
        }

        public TransitionsHelper To(State state)
        {
            Assert.IsTrue(StatesCollection.List.Contains(state));
            _to = state;
            return this;
        }
        #endregion

        public bool IsExist()
        {
            var transition = GetTransition(_from, _to);
            Reset();
            return transition != null;
        }

        public TransitionsHelper Remove()
        {
            var transition = GetTransition(_from, _to);
            Assert.IsNotNull(transition);
            _from.RemoveTransition(transition);
            Reset();
            return this;
        }

        [NotNull]
        public Transition Get()
        {
            var transition = GetTransition(_from, _to);
            Assert.IsNotNull(transition);
            Reset();
            return transition;
        }

        public TransitionsHelper Set(Func<bool> condition)
        {
            Set(Condition.Get(condition));
            return this;
        }

        public TransitionsHelper Set(ICondition condition)
        {
            var value = GetTransition(_from, _to);
            Assert.IsNull(value);
            var transition = new Transition(_to, condition);
            _from.AddTransition(transition);
            Reset();
            return this;
        }

        [CanBeNull]
        private Transition GetTransition([NotNull] State from, [NotNull] State to)
        {
            Assert.IsNotNull(from);
            Assert.IsNotNull(to);
            foreach (var state in StatesCollection.List)
            {
                if (state != _from)
                    continue;

                foreach (var transition in state.TransitionsList)
                {
                    if (transition.Target == _to)
                    {
                        return transition;
                    }
                }
            }

            return null;
        }

        private void Reset()
        {
            _from = null;
            _to = null;
        }
    }
}