using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace InvisibleHand.Items
{

    //https://www.psclistens.com/insight/blog/quickly-build-a-business-rules-engine-using-c-and-lambda-expression-trees/

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

    /// The pre-compiled rules type
    ///
    public class PrecompiledRules
    {
        ///
        /// A method used to precompile rules for a provided type
        ///
        public static List<Func<T, bool>> CompileRule<T>(List<T> targetEntity, List<Rule> rules)
        {
            var compiledRules = new List<Func<T, bool>>();

            // Loop through the rules and compile them against the properties of the supplied shallow object
            rules.ForEach(rule =>
            {
                var genericType = Expression.Parameter(typeof(T));
                var key = MemberExpression.Property(genericType, rule.ComparisonPredicate);
                var propertyType = typeof(T).GetProperty(rule.ComparisonPredicate).PropertyType;
                var value = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, propertyType));
                var binaryExpression = Expression.MakeBinary(rule.ComparisonOperator, key, value);

                compiledRules.Add(Expression.Lambda<Func<T, bool>>(binaryExpression, genericType).Compile());
            });

            // Return the compiled rules to the caller
            return compiledRules;
        }
    }

    // Examples:
    // List<Rule> rules = new List<Rule>
    // {
     // Create some rules using LINQ.ExpressionTypes for the comparison operators
        //      new Rule ( "Year", ExpressionType.GreaterThan, "2012"),
        //      new Rule ( "Make", ExpressionType.Equal, "El Diablo"),
        //      new Rule ( "Model", ExpressionType.Equal, "Torch" )
        // };
        //
        // var compiledMakeModelYearRules= PrecompiledRules.CompileRule(new List<ICar>(), rules);
        //
        // // Create a list to house your test cars
        // List cars = new List();
        //
        // // Create a car that's year and model fail the rules validations
        // Car car1_Bad = new Car {
        //     Year = 2011,
        //     Make = "El Diablo",
        //     Model = "Torche"
        // };
        //
        // // Create a car that meets all the conditions of the rules validations
        // Car car2_Good = new Car
        // {
        //     Year = 2015,
        //     Make = "El Diablo",
        //     Model = "Torch"
        // };
        //
        // // Add your cars to the list
        // cars.Add(car1_Bad);
        // cars.Add(car2_Good);
        //
        // // Iterate through your list of cars to see which ones meet the rules vs. the ones that don't
        // cars.ForEach(car => {
        //     if (compiledMakeModelYearRules.TakeWhile(rule => rule(car)).Count() > 0)
        //     {
        //         Console.WriteLine(string.Concat("Car model: ", car.Model, " Passed the compiled rules engine check!"));
        //     }
        //     else
        //     {
        //         Console.WriteLine(string.Concat("Car model: ", car.Model, " Failed the compiled rules engine check!"));
        //     }
        // });
        //
}
