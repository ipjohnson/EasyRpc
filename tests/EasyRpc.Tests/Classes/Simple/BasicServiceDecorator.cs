﻿namespace EasyRpc.Tests.Classes.Simple
{
    public class BasicServiceDecorator : IBasicService
    {
        private readonly IBasicService _basicService;

        public BasicServiceDecorator(IBasicService basicService)
        {
            _basicService = basicService;
        }

        public int Count
        {
            get { return _basicService.Count; }
            set { _basicService.Count = value; }
        }

        public int TestMethod()
        {
            return _basicService.TestMethod();
        }
    }
}
