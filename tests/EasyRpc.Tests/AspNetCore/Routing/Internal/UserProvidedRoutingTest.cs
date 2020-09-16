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
            return RouteListString.Split("\r\n").ToList();
        }

        private static readonly string RouteListString =
@"/EquationEvaluationService/Equals
/EquationEvaluationService/Evaluate
/EquationEvaluationService/GetHashCode
/EquationEvaluationService/GetType
/EquationEvaluationService/ToString
/EquationRangeCalculatorService/Equals
/EquationRangeCalculatorService/GetHashCode
/EquationRangeCalculatorService/GetRanges
/EquationRangeCalculatorService/GetType
/EquationRangeCalculatorService/ToString
/EquationService/Create
/EquationService/Delete
/EquationService/Equals
/EquationService/Get
/EquationService/GetAll
/EquationService/GetByExpression
/EquationService/GetHashCode
/EquationService/GetType
/EquationService/ToString
/EquationService/Update
/IEquationEvaluationService/Evaluate
/IEquationRangeCalculatorService/GetRanges
/IEquationService/Create
/IEquationService/Delete
/IEquationService/Get
/IEquationService/GetAll
/IEquationService/GetByExpression
/IEquationService/Update
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
