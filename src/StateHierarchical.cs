using GoodCat.Fsm.FsmHelpers;

namespace GoodCat.Fsm
{
    public class StateHierarchical : State
    {
        public readonly StatesCollection StatesCollection;

        public StateHierarchical(FSM fsm, string name = "") : base(name)
        {
            StatesCollection = new StatesCollection(fsm, this);
        }

        protected override void OnEnable()
        {
            StatesCollection.StateStartDefault.StartState();
        }

        protected override bool OnUpdate() => false;

        protected override void OnDisable()
        {
            foreach (var state in StatesCollection.List)
            {
                state.Disable();
            }
        }

        public override string ToString() => $"State: {Name}";
    }
}