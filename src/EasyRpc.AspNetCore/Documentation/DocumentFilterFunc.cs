using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public class DocumentFilterFunc : IDocumentFilter
    {
        private readonly Action<DocumentFilterContext> _filterAction;

        public DocumentFilterFunc(Action<DocumentFilterContext> filterAction)
        {
            _filterAction = filterAction;
        }

        /// <inheritdoc />
        public void Apply(DocumentFilterContext context)
        {
            _filterAction(context);
        }
    }
}
