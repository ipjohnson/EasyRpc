using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.Routing
{
    public interface IRegisteredEndPoints
    {
        IReadOnlyList<IEndPointMethodHandler> EndPoints { get; set; }
    }

    public class RegisteredEndPoints : IRegisteredEndPoints
    {
        private IReadOnlyList<IEndPointMethodHandler> _endPoints;

        public IReadOnlyList<IEndPointMethodHandler> EndPoints
        {
            get => _endPoints;
            set
            {
                if (value != null)
                {
                    _endPoints = value;
                }
            }
        }
    }
}
