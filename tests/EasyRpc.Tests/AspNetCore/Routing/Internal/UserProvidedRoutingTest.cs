using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Routing.Internal
{
    public class UserProvidedRoutingTest : BaseRequestTest
    {
        private Random _random = new Random();
        private List<string> _routes;

        #region Tests

        [Fact]
        public async Task Routing_Internal_UserProvidedRoutingTest()
        {
            _routes = GenerateRouteList();

            foreach (var route in _routes)
            {
                var response = await Get(route);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    throw new Exception($"Failed on route {route} response {content}");
                }

                var stringValue = await Deserialize<string>(response);

                Assert.Equal(route, stringValue);
            }
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            foreach (var route in _routes)
            {
                api.GetMethod(route, () => route);
            }
        }

        private List<string> GenerateRouteList()
        {
            return RouteListString.Split(Environment.NewLine).ToList();
        }

        private static string RouteListString =
@"/FormulaEvaluationService/Equals
/FormulaEvaluationService/Evaluate
/FormulaEvaluationService/GetHashCode
/FormulaEvaluationService/GetType
/FormulaEvaluationService/ToString
/FormulaRangeCalculatorService/Equals
/FormulaRangeCalculatorService/GetHashCode
/FormulaRangeCalculatorService/GetRanges
/FormulaRangeCalculatorService/GetType
/FormulaRangeCalculatorService/ToString
/FormulaService/Create
/FormulaService/Delete
/FormulaService/Equals
/FormulaService/Get
/FormulaService/GetAll
/FormulaService/GetByExpression
/FormulaService/GetHashCode
/FormulaService/GetType
/FormulaService/ToString
/FormulaService/Update
/IFormulaEvaluationService/Evaluate
/IFormulaRangeCalculatorService/GetRanges
/IFormulaService/Create
/IFormulaService/Delete
/IFormulaService/Get
/IFormulaService/GetAll
/IFormulaService/GetByExpression
/IFormulaService/Update
/IIndexService/GetAll
/IIndexService/GetValues
/IListService/GetAll
/IndexService/Equals
/IndexService/GetAll
/IndexService/GetHashCode
/IndexService/GetType
/IndexService/GetValues
/IndexService/ToString
/ITradeService/Get
/ITradeService/GetAll
/ITradeService/Save
/ListService/Equals
/ListService/GetAll
/ListService/GetHashCode
/ListService/GetType
/ListService/ToString
/TradeService/Equals
/TradeService/Get
/TradeService/GetAll
/TradeService/GetHashCode
/TradeService/GetType
/TradeService/Save
/TradeService/ToString";

        #endregion
    }
}
