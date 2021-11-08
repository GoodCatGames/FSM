using System;
using GoodCat.Conditions;
using JetBrains.Annotations;

namespace GoodCat.Fsm
{
    public class Transition
    {
        [NotNull] public readonly State Target;
        [NotNull] public ICondition Condition;

        public Transition([NotNull] State target, [NotNull] ICondition condition)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }
    }
}