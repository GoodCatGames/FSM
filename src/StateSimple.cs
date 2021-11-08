using System;
using JetBrains.Annotations;

namespace GoodCat.Fsm
{
    public class StateSimple : State
    {
        [CanBeNull] private readonly Action _onEnableAction;
        
        [CanBeNull] private readonly Action _onUpdateAction;
        
        [CanBeNull] private readonly Action _onDisableAction;

        public StateSimple([NotNull] string name, 
            [CanBeNull] Action onEnableAction,
            [CanBeNull] Action onUpdateAction, 
            [CanBeNull] Action onDisableAction) : base(name)
        {
            _onEnableAction = onEnableAction;
            _onUpdateAction = onUpdateAction;
            _onDisableAction = onDisableAction;
        }

        protected override void OnEnable() => _onEnableAction?.Invoke();

        protected override bool OnUpdate()
        {
            if (DoTransitionIfNeed())
                return true;
            _onUpdateAction?.Invoke();
            return false;
        }

        protected override void OnDisable() =>
            _onDisableAction?.Invoke();
    }
}