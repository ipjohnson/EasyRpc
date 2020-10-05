using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;


namespace EasyRpc.AspNetCore.Routing
{
    /// <summary>
    /// Service that provides an internal routing method path => IEndPointHandler
    /// </summary>
    public interface IEndPointRouteBuilder
    {
        /// <summary>
        /// Builds routing function given a dictionary of handlers
        /// </summary>
        /// <param name="handlers"></param>
        /// <returns></returns>
        Func<string, IEndPointHandler> BuildRouteFunc(IDictionary<string, IEndPointHandler> handlers);
    }

    /// <summary>
    /// Internal route building service
    /// </summary>
    public class EndPointRouteBuilder : IEndPointRouteBuilder
    {
        private readonly ParameterExpression _pathParameter = Expression.Parameter(typeof(string), "path");
        private readonly PropertyInfo _charsProperty = typeof(string).GetProperty("Chars");
        private readonly PropertyInfo _lengthProperty = typeof(string).GetProperty("Length");

        /// <inheritdoc />
        public Func<string, IEndPointHandler> BuildRouteFunc(IDictionary<string, IEndPointHandler> handlers)
        {
            List<KeyValuePair<string, IEndPointHandler>> pathList = null;

            if (handlers.Count == 0)
            {
                return s => null;
            }

            pathList = handlers.ToList();

            pathList.Sort((x, y) => Comparer<string>.Default.Compare(x.Key, y.Key));

            return GenerateStartingMethod(pathList);
        }

        private Func<string, IEndPointHandler> GenerateStartingMethod(List<KeyValuePair<string, IEndPointHandler>> pathList)
        {
            var searchMethod = GenerateSearchMethod(pathList, 0, 0, pathList.Count);

            return GenerateDelegate(searchMethod);
        }

        private Expression GenerateSearchMethod(List<KeyValuePair<string, IEndPointHandler>> pathList, int currentStringIndex,
            int pathListStart, int pathListEnd, int level = 0)
        {
            var distinctCharacters = DistinctCharacters(pathList, currentStringIndex, pathListStart, pathListEnd);

            if (distinctCharacters == 1)
            {
                return GenerateLongMatch(pathList, currentStringIndex, pathListStart, pathListEnd, level);
            }

            if (pathListEnd - pathListStart < 10)
            {
                return GenerateNodeMethod(pathList, currentStringIndex, pathListStart, pathListEnd);
            }

            if (distinctCharacters < 5)
            {
                return GenerateIfBranchMethod(pathList, currentStringIndex, pathListStart, pathListEnd, level, distinctCharacters);
            }

            return GenerateSwitch(pathList, currentStringIndex, pathListStart, pathListEnd);
        }

        private Expression GenerateLongMatch(List<KeyValuePair<string, IEndPointHandler>> pathList, int currentStringIndex, int pathListStart, int pathListEnd, int level)
        {
            var longMatch = PathMatchCount(pathList, currentStringIndex, pathListStart, pathListEnd);

            // this is to handle the case where the path ends with a / and it should still match
            if (pathListEnd - pathListStart == 1 && pathList[pathListStart].Key.EndsWith("/"))
            {
                longMatch--;
            }

            var comparisonStatement =
                GenerateStringComparisonStatements(pathList[pathListStart].Key, currentStringIndex, longMatch, longMatch);

            Expression matchLogic = null;

            if (pathListEnd - pathListStart > 1)
            {
                matchLogic = GenerateSearchMethod(pathList, currentStringIndex + longMatch, pathListStart, pathListEnd, level);
            }
            else
            {
                matchLogic = Expression.Constant(pathList[pathListStart].Value, typeof(IEndPointHandler));
            }

            return Expression.Condition(comparisonStatement, matchLogic, Expression.Constant(null, typeof(IEndPointHandler)));
        }

        private BinaryExpression GenerateStringComparisonStatements(string path, int currentStringIndex, int stringLength, int testLength)
        {
            Expression currentExpression = null;

            for (int i = 0; i < stringLength; i++)
            {
                var index = currentStringIndex + i;

                var testExpression = GenerateCharacterComparisonStatement(index, path[index]);

                if (currentExpression == null)
                {
                    currentExpression = testExpression;
                }
                else
                {
                    currentExpression = Expression.AndAlso(testExpression, currentExpression);
                }
            }

            var testStatement = Expression.GreaterThanOrEqual(Expression.Property(_pathParameter, _lengthProperty),
                Expression.Constant(currentStringIndex + testLength));

            if (currentExpression == null)
            {
                return testStatement;
            }

            return Expression.AndAlso(testStatement, currentExpression);
        }

        private Expression GenerateCharacterComparisonStatement(int index, char compare)
        {
            var lower = char.ToLowerInvariant(compare);
            var upper = char.ToUpperInvariant(compare);

            if (lower == upper)
            {
                return Expression.Equal(Expression.Property(_pathParameter, _charsProperty, Expression.Constant(index)), Expression.Constant(compare));
            }

            var lowerCompare = Expression.Equal(Expression.Property(_pathParameter, _charsProperty, Expression.Constant(index)), Expression.Constant(lower));
            var upperCompare = Expression.Equal(Expression.Property(_pathParameter, _charsProperty, Expression.Constant(index)), Expression.Constant(upper));

            return Expression.OrElse(lowerCompare, upperCompare);
        }

        private Expression GenerateIfBranchMethod(List<KeyValuePair<string, IEndPointHandler>> pathList, int currentStringIndex,
            int pathListStart, int pathListEnd, int level, int distinctCharacters)
        {
            var currentChar = char.ToLowerInvariant(pathList[pathListStart].Key[currentStringIndex]);
            var currentStart = pathListStart;
            Expression currentExpression = Expression.Constant(null, typeof(IEndPointHandler));

            for (var i = pathListStart; i < pathListEnd; i++)
            {
                var path = pathList[i].Key;
                var newChar = char.ToLowerInvariant(path[currentStringIndex]);

                if (newChar == currentChar)
                {
                    continue;
                }

                var newExpression = GenerateSearchMethod(pathList, currentStringIndex + 1, currentStart, i, level);

                var comparison = GenerateStringComparisonStatements(pathList[currentStart].Key, currentStringIndex, 1, 1);

                currentExpression = Expression.Condition(comparison, newExpression, currentExpression);

                currentStart = i;
                currentChar = newChar;
            }

            var lastExpression = GenerateSearchMethod(pathList, currentStringIndex + 1, currentStart,
                pathListEnd, level);

            var lastComparison = GenerateStringComparisonStatements(pathList[currentStart].Key, currentStringIndex, 1, 1);

            currentExpression = Expression.Condition(lastComparison, lastExpression, currentExpression);

            return currentExpression;
        }

        private Expression GenerateNodeMethod(List<KeyValuePair<string, IEndPointHandler>> pathList, int currentStringIndex,
            int pathListStart, int pathListEnd)
        {
            Expression currentExpression = Expression.Constant(null, typeof(IEndPointHandler));

            var exactMatch = false;

            for (var i = pathListStart; i < pathListEnd; i++)
            {
                var path = pathList[i].Key;
                var value = pathList[i].Value;
                

                BinaryExpression comparison = null;

                if (path.Length - currentStringIndex == 0)
                {
                    exactMatch = true;

                    if (value.SupportsLongerPaths)
                    {
                        comparison = Expression.GreaterThanOrEqual(Expression.Property(_pathParameter, _lengthProperty),
                            Expression.Constant(path.Length));
                    }
                    else
                    {
                        comparison = Expression.Equal(Expression.Property(_pathParameter, _lengthProperty),
                            Expression.Constant(path.Length));
                    }
                }
                else
                {
                    var length = path.Length - currentStringIndex;
                    var testLength = length;

                    if (exactMatch)
                    {
                        testLength++;
                    }
                    else
                    {
                        // this is to handle the case where the path ends with a / and it should still match
                        if (path.EndsWith("/"))
                        {
                            length--;
                        }

                        testLength = length;
                    }

                    comparison = GenerateStringComparisonStatements(path, currentStringIndex, length, testLength);
                }

                currentExpression = Expression.Condition(comparison, Expression.Constant(value, typeof(IEndPointHandler)), currentExpression);
            }

            return currentExpression;
        }

        private Expression GenerateSwitch(List<KeyValuePair<string, IEndPointHandler>> pathList, int currentStringIndex, int pathListStart, int pathListEnd)
        {
            var currentChar = pathList[pathListStart].Key[currentStringIndex];
            var currentStart = pathListStart;

            var caseList = new List<SwitchCase>();

            for (var i = pathListStart; i < pathListEnd; i++)
            {
                var newValue = pathList[i].Key[currentStringIndex];

                if (newValue == currentChar)
                {
                    continue;
                }

                caseList.Add(CreateCaseStatements(pathList, currentChar, currentStringIndex, currentStart, i));

                currentChar = newValue;
                currentStart = i;
            }

            caseList.Add(CreateCaseStatements(pathList, currentChar, currentStringIndex, currentStart, pathListEnd));

            return Expression.Switch(Expression.Property(_pathParameter, _charsProperty, Expression.Constant(currentStringIndex)), Expression.Constant(null, typeof(IEndPointHandler)), caseList.ToArray());
        }

        private SwitchCase CreateCaseStatements(List<KeyValuePair<string, IEndPointHandler>> pathList, char currentChar,
            int currentStringIndex, int start, int end)
        {
            var expression = GenerateSearchMethod(pathList, currentStringIndex + 1, start, end);

            var compiledDelegate = GenerateDelegate(expression);
            var target = compiledDelegate.Target;

            var callExpression = Expression.Invoke(Expression.Constant(compiledDelegate), _pathParameter);

            var lowerCase = char.ToLowerInvariant(currentChar);
            var upperCase = char.ToUpperInvariant(currentChar);

            if (lowerCase == upperCase)
            {
                return Expression.SwitchCase(callExpression, Expression.Constant(currentChar));
            }
            else
            {
                return Expression.SwitchCase(callExpression, Expression.Constant(upperCase), Expression.Constant(lowerCase));
            }
        }

        private int PathMatchCount(List<KeyValuePair<string, IEndPointHandler>> pathList, int currentStringIndex, int pathListStart,
            int pathListEnd)
        {
            var count = 0;

            while (true)
            {
                var matches = true;
                var currentCheckString = pathList[pathListStart].Key;

                if (currentStringIndex >= currentCheckString.Length)
                {
                    break;
                }

                var currentChar = char.ToLowerInvariant(currentCheckString[currentStringIndex]);

                for (var i = pathListStart; i < pathListEnd; i++)
                {
                    currentCheckString = pathList[i].Key;

                    if (currentCheckString.Length < currentStringIndex ||
                        char.ToLowerInvariant(currentCheckString[currentStringIndex]) != currentChar)
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    count++;
                    currentStringIndex++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }

        private int DistinctCharacters(List<KeyValuePair<string, IEndPointHandler>> pathList, int currentStringIndex,
            int pathListStart,
            int pathListEnd)
        {
            if (currentStringIndex >= pathList[pathListStart].Key.Length)
            {
                return 0;
            }

            var currentChar = pathList[pathListStart].Key[currentStringIndex];
            var count = 1;

            for (var i = pathListStart; i < pathListEnd; i++)
            {
                if (pathList[i].Key[currentStringIndex] == currentChar)
                {
                    continue;
                }

                currentChar = pathList[i].Key[currentStringIndex];
                count++;
            }

            return count;
        }

        private Func<string, IEndPointHandler> GenerateDelegate(Expression expression)
        {
            return Expression.Lambda<Func<string, IEndPointHandler>>(expression, _pathParameter).Compile();
        }
    }
}
