using System;

namespace InvisibleHand.Utils
{
    /// represents the root of a decision tree that evaluates
    /// objects of type T; Evaluate(T) will use the test function to determine which of
    /// OnYes.Evaluate(T) or OnNo.Evaluate(T) to call to continue the decision process
    public interface IDecision<T>
    {
        IDecision<T> OnYes { get; }
        IDecision<T> OnNo { get; }

        Func<T, bool> Test { get; }

        bool Evaluate(T subject);
    }
}
