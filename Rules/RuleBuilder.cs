using System;
using System.Collections.Generic;
// using System.Linq;
using System.Linq.Expressions;


namespace InvisibleHand.Rules
{
    /// The pre-compiled rules type
    ///
    public class RuleBuilder
    {
        ///
        /// A method used to precompile rules for a provided type
        ///
        public static List<Func<T, bool>> CompileRule<T>(List<T> targetEntity, List<Rule> rules)
        {
            // storage for the compiled rules
            var compiledRules = new List<Func<T, bool>>();
            // get the type of the entity these rules are meant to govern (constant for each rule)
            var genericType = Expression.Parameter(typeof(T));

            // Loop through the rules and compile them against the properties of the supplied shallow object
            rules.ForEach(rule =>
            {
                // find the object Property specified by the predicate (i.e. the property name)
                var key = MemberExpression.Property(genericType, rule.ComparisonPredicate);
                // get the Type of that property
                var propertyType = typeof(T).GetProperty(rule.ComparisonPredicate).PropertyType;
                // get the value to be checked as a member of the property's type
                var value = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, propertyType));
                // create the 2-sided comparison expression btw. the prop value and the supplied constant
                var binaryExpression = Expression.MakeBinary(rule.ComparisonOperator, key, value);

                // compile the expression and add it to the list of compiled rules
                compiledRules.Add(Expression.Lambda<Func<T, bool>>(binaryExpression, genericType).Compile());
            });

            // Return the compiled rules to the caller
            return compiledRules;
        }

        /// compiles rules that perform a comparison operation on the properties of two objects of the same type
        // public static List<Func<T, T, int>> CompileVsRules<T>(List<T> targetEntity, List<string> predicates, out List<Expression<Func<T, T, int>>> expressions)
        public static List<Func<T, T, int>> CompileVsRules<T>(List<T> targetEntity, List<string> predicates)
        {
            // storage for the compiled rules
            var compiledRules = new List<Func<T, T, int>>();

            // var _expressions = new List<Expression<Func<T, T, int>>>();
            // get the type of the entity these rules are meant to govern (constant for each rule)
            // var genericType = Expression.Parameter(typeof(T));

            var item1 = Expression.Parameter(typeof(T), "item1");
            var item2 = Expression.Parameter(typeof(T), "item2");

            // Loop through the rules and compile them against the properties of the supplied shallow object
            predicates.ForEach(rule =>
            {
                // find the object Property specified by the predicate (i.e. the property name)
                var prop = MemberExpression.PropertyOrField(item1, rule);
                var prop2 = MemberExpression.PropertyOrField(item2, rule);
                // get the Type of that property
                // var propertyType = typeof(T).GetProperty(rule.ComparisonPredicate).PropertyType;


                var compareExpression = Expression.Call(
                    prop,
                    prop.Type.GetMethod("CompareTo", new[] { prop2.Type }),
                    prop2);

                // get the value to be checked as a member of the property's type
                // var value = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, propertyType));
                // create the 2-sided comparison expression btw. the prop value and the supplied constant
                // var binaryExpression = Expression.MakeBinary(rule.ComparisonOperator, prop, value);

                // compile the expression and add it to the list of compiled rules
                // compiledRules.Add(Expression.Lambda<Func<T, bool>>(binaryExpression, genericType).Compile());
                // _expressions.Add(Expression.Lambda<Func<T, T, int>>(compareExpression, new[] {item1, item2}));
                compiledRules.Add(Expression.Lambda<Func<T, T, int>>(compareExpression, new[] {item1, item2}).Compile());
            });

            // expressions = _expressions;

            // Return the compiled rules to the caller
            return compiledRules;
        }
    }


    // public class Test
    // {
    //
    //     internal interface ICar
    //     {
    //         int Year { get;  }
    //         string Make { get;  }
    //         string Model { get;  }
    //     }
    //     internal class Car : ICar
    //     {
    //         public int Year { get; set; }
    //         public string Make { get; set; }
    //         public string Model { get; set; }
    //     }
    //
    //     static void Main()
    //     {
    //         // Examples:
    //         List<Rule> rules = new List<Rule>
    //         {
    //         //  Create some rules using LINQ.ExpressionTypes for the comparison operators
    //              new Rule ( "Year", ">", "2012"),
    //              new Rule ( "Make", "==", "El Diablo"),
    //              new Rule ( "Model", "==", "Torch" )
    //         };
    //
    //
    //         var compiledMakeModelYearRules = RuleBuilder.CompileRule(new List<ICar>(), rules);
    //
    //
    //         // Create a list to house your test cars
    //         List<Car> cars = new List<Car>();
    //
    //         // Create a car that's year and model fail the rules validations
    //         Car car1_Bad = new Car {
    //             Year = 2013,
    //             Make = "El Diablo",
    //             Model = "Torche"
    //         };
    //
    //         // Create a car that meets all the conditions of the rules validations
    //         Car car2_Good = new Car
    //         {
    //             Year = 2015,
    //             Make = "El Diablo",
    //             Model = "Torch"
    //         };
    //
    //         // Add your cars to the list
    //         cars.Add(car1_Bad);
    //         cars.Add(car2_Good);
    //
    //         // Iterate through your list of cars to see which ones meet the rules vs. the ones that don't
    //         cars.ForEach(car =>
    //         {
    //             // if (compiledMakeModelYearRules.TakeWhile(rule => rule(car)).Count() > 0)
    //
    //             if (compiledMakeModelYearRules.All(rule => rule(car)))
    //             {
    //                 Console.WriteLine(string.Concat("Car model: ", car.Model, " Passed the compiled rules engine check!"));
    //             }
    //             else
    //             {
    //                 Console.WriteLine(string.Concat("Car model: ", car.Model, " Failed the compiled rules engine check!"));
    //             }
    //         });
    //
    //     }
    // }

}
