using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public class OperationFilterFunc : IOperationFilter
    {
        private readonly Action<OperationFilterContext> _filterAction;

        public OperationFilterFunc(Action<OperationFilterContext> filterAction)
        {
            _filterAction = filterAction;
        }

        /// <inheritdoc />
        public void Apply(OperationFilterContext context)
        {
            _filterAction(context);
        }
    }
}
