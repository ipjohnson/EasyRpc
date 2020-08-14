﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EasyRpc.AspNetCore
{
    public partial interface IApiConfiguration
    {
        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration GetMethod<TResult>(string path, Expression<Func<TResult>> method);


        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration GetMethod<TArg1, TResult>(string path, Expression<Func<TArg1, TResult>> method);


        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration GetMethod<TArg1, TArg2, TResult>(string path, Expression<Func<TArg1, TArg2, TResult>> method);


        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration GetMethod<TArg1, TArg2, TArg3, TResult>(string path, Expression<Func<TArg1, TArg2, TArg3, TResult>> method);

        /// <summary>
        /// Simple post method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="postExpression"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration PostMethod<TArg1, TResult>(string path, Expression<Func<TArg1, TResult>> postExpression);

        /// <summary>
        /// Simple post method with two arg
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="postExpression"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration PostMethod<TArg1, TArg2, TResult>(string path, Expression<Func<TArg1, TArg2, TResult>> postExpression);

        /// <summary>
        /// Simple post method with two arg
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="path"></param>
        /// <param name="postExpression"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration PostMethod<TArg1, TArg2, TArg3, TResult>(string path, Expression<Func<TArg1, TArg2, TArg3, TResult>> postExpression);


        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpMethod"></param>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration HttpMethod<TResult>(string httpMethod, string path, Expression<Func<TResult>> method);


        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <param name="httpMethod"></param>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration HttpMethod<TArg1, TResult>(string httpMethod, string path, Expression<Func<TArg1, TResult>> method);


        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <param name="httpMethod"></param>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration HttpMethod<TArg1, TArg2, TResult>(string httpMethod, string path, Expression<Func<TArg1, TArg2, TResult>> method);


        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="httpMethod"></param>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration HttpMethod<TArg1, TArg2, TArg3, TResult>(string httpMethod, string path, Expression<Func<TArg1, TArg2, TArg3, TResult>> method);

    }
}
