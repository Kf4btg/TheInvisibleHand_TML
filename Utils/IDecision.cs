using System;
using System.Collections.Generic;

namespace InvisibleHand.Utils
{

    public interface IDecision<T>
    {
        IList<IDecision<T>> Branches { get; }

        Func<T, int> NextBranch { get; }

        IDecision<T> Evaluate(T t);
    }

    public class BinaryDecision<T> : IDecision<T>
    {
        public IDecision<T> OnYes { get; set; }
        public IDecision<T> OnNo { get; set; }
        public Func<T, bool> Test { get; set; }

        public IList<IDecision<T>> Branches { get; private set; }

        public Func<T, int> NextBranch { get; private set; }

        public BinaryDecision()
        {
            Branches = new List<IDecision<T>> { OnNo , OnYes };
            this.NextBranch = (t) => this.Test(t) ? 1 : 0;
        }

        public virtual IDecision<T> Evaluate(T t)
        {
            return this.Branches[this.NextBranch(t)];
        }
    }

    public class Decision<T> : IDecision<T>
    {
        public IList<IDecision<T>> Branches { get; set; }
        public Func<T, int> NextBranch { get; set; }

        public virtual IDecision<T> Evaluate(T t)
        {
            return this.Branches[this.NextBranch(t)];
        }
    }
}
