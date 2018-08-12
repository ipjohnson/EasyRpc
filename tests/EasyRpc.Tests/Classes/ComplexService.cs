using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;

namespace EasyRpc.Tests.Classes
{
    public interface IComplexService
    {
        ResultObject Add(ComplexObject complex);

        List<ResultObject> AddList(List<ComplexObject> complex);

        Task<List<ResultObject>> AsyncAddList(List<ComplexObject> complex);
    }

    public class ComplexObject
    {
        public int A { get; set; }

        public int B { get; set; }
    }

    public class ResultObject
    {
        public int Result { get; set; }
    }


    public class ComplexService : IComplexService
    {

        public ResultObject ReturnNull(ComplexObject complex)
        {
            return null;
        }

        public ResultObject Add(ComplexObject complex)
        {
            return new ResultObject { Result = complex.A + complex.B };
        }

        public List<ResultObject> AddList(List<ComplexObject> complex)
        {
            return complex.Select(o => new ResultObject { Result = o.A + o.B }).ToList();
        }

        public Task<List<ResultObject>> AsyncAddList(List<ComplexObject> complex)
        {
            return Task.FromResult(complex.Select(o => new ResultObject { Result = o.A + o.B }).ToList());
        }

        public string ReturnString(int stringLength)
        {
            return new String('a', stringLength);
        }

        public Task<string> AsyncReturnString(int stringLength)
        {
            return Task.FromResult(new String('a', stringLength));
        }

        public ResponseMessage ReturnResponseMessage(bool compress)
        {
            return new ResponseMessage<string>(new string('a', 1000)) { CanCompress = compress };
        }
        
        public ResponseMessage AsyncReturnResponseMessage(bool compress)
        {
            return new ResponseMessage<string>(new string('a', 1000)) { CanCompress = compress };
        }
    }
}
