namespace EasyRpc.TestApp.FSharp.Services

/// <summary>
/// Integer math service
/// </summary>
type IntegerMathService() =

    /// <summary>
    /// Add method
    /// </summary>
    /// <param name="x">x parameter</param>
    /// <param name="y">y parameter</param>
    member this.Add (x:int)(y:int) = x + y

    /// <summary>
    /// Subtract method
    /// </summary>
    /// <param name="x">x parameter</param>
    /// <param name="y">y parameter</param>
    member this.Subtract (x:int)(y:int) = x - y
