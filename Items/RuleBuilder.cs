using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace InvisibleHand.Items
{
    /// The Rule type
    public class Rule
    {
        ///
        /// Denotes the rules predictate (e.g. Name); comparison operator(e.g. ExpressionType.GreaterThan); value (e.g. "Cole")
        ///
        public string ComparisonPredicate { get; set; }
        public ExpressionType ComparisonOperator { get; set; }
        public string ComparisonValue { get; set; }

        /// Constructor for the Rule 'method'
        public Rule(string comparisonPredicate, ExpressionType comparisonOperator, string comparisonValue)
        {
            ComparisonPredicate = comparisonPredicate;
            ComparisonOperator = comparisonOperator;
            ComparisonValue = comparisonValue;
        }

        /// Constructor for the Rule 'method'
        public Rule(string comparisonPredicate, string comparisonOperator, string comparisonValue)
        {
            ComparisonPredicate = comparisonPredicate;
            ComparisonOperator = getComparisonOperator(comparisonOperator);
            ComparisonValue = comparisonValue;
        }

        private static IDictionary<string, ExpressionType> SymbolToOperator;
        static Rule()
        {
            SymbolToOperator = new Dictionary<string, ExpressionType>()
            {
                {"&", ExpressionType.And},
                {"&&", ExpressionType.AndAlso},
                {"&=", ExpressionType.AndAssign},
                {"|", ExpressionType.Or},
                {"||", ExpressionType.OrElse},
                {"|=", ExpressionType.OrAssign},
                {"==", ExpressionType.Equal},
                {"^", ExpressionType.ExclusiveOr},
                {"^=", ExpressionType.ExclusiveOrAssign},
                {">", ExpressionType.GreaterThan},
                {">=", ExpressionType.GreaterThanOrEqual},
                {"<", ExpressionType.LessThan},
                {"<=", ExpressionType.LessThanOrEqual},
                {"=", ExpressionType.Assign},
                {"()", ExpressionType.Call},
                {"=>", ExpressionType.Lambda},
                {"?:", ExpressionType.Conditional},
                {"??", ExpressionType.Coalesce},
                {"C", ExpressionType.Constant},
                {"len", ExpressionType.ArrayLength},
                {"[]", ExpressionType.ArrayIndex},
                {"conv", ExpressionType.Convert},
                {"(conv)", ExpressionType.ConvertChecked},
                {"<<", ExpressionType.LeftShift},
                {">>", ExpressionType.RightShift},
                {"[]{}", ExpressionType.ListInit},
                {".", ExpressionType.MemberAccess},
                {"new{}", ExpressionType.MemberInit},
                {"-x", ExpressionType.Negate},
                {"(-x)", ExpressionType.NegateChecked},
                {"new", ExpressionType.New},
                {"new[]{}", ExpressionType.NewArrayInit},
                {"new[,]", ExpressionType.NewArrayBounds},
                {"!", ExpressionType.Not},
                {"!=", ExpressionType.NotEqual},
                {"param", ExpressionType.Parameter},
                {"**", ExpressionType.Power},
                {"\"", ExpressionType.Quote},
                {"+", ExpressionType.Add},
                {"(+)", ExpressionType.AddChecked},
                {"-", ExpressionType.Subtract},
                {"(-)", ExpressionType.SubtractChecked},
                {"%", ExpressionType.Modulo},
                {"*", ExpressionType.Multiply},
                {"(*)", ExpressionType.MultiplyChecked},
                {"/", ExpressionType.Divide},
                {"as", ExpressionType.TypeAs},
                {"is", ExpressionType.TypeIs},
                {"{}", ExpressionType.Block},
                {"-1", ExpressionType.Decrement},
                {"dyn", ExpressionType.Dynamic},
                {"def", ExpressionType.Default},
                {"ext", ExpressionType.Extension},
                {"goto", ExpressionType.Goto},
                {"+1", ExpressionType.Increment},
                {"[x]", ExpressionType.Index},
                {"...", ExpressionType.Label},
                {"var[]", ExpressionType.RuntimeVariables},
                {"loop", ExpressionType.Loop},
                {"switch", ExpressionType.Switch},
                {"throw", ExpressionType.Throw},
                {"Try", ExpressionType.Try},
                {"-box", ExpressionType.Unbox},
                {"+=", ExpressionType.AddAssign},
                {"(+=)", ExpressionType.AddAssignChecked},
                {"/=", ExpressionType.DivideAssign},
                {"<<=", ExpressionType.LeftShiftAssign},
                {"%=", ExpressionType.ModuloAssign},
                {"*=", ExpressionType.MultiplyAssign},
                {"(*=)", ExpressionType.MultiplyAssignChecked},
                {"**=", ExpressionType.PowerAssign},
                {">>=", ExpressionType.RightShiftAssign},
                {"-=", ExpressionType.SubtractAssign},
                {"(-=)", ExpressionType.SubtractAssignChecked},
                {"++x", ExpressionType.PreIncrementAssign},
                {"--x", ExpressionType.PreDecrementAssign},
                {"x++", ExpressionType.PostIncrementAssign},
                {"x--", ExpressionType.PostDecrementAssign},
                {"===", ExpressionType.TypeEqual},
                {"~", ExpressionType.OnesComplement},
                {"T", ExpressionType.IsTrue},
                {"F", ExpressionType.IsFalse},
                {"debug", ExpressionType.DebugInfo},
            };
        }

        private static ExpressionType getComparisonOperator(string symbol)
        {
            return SymbolToOperator[symbol];
        }




    }
}
