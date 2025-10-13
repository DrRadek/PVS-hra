using System;

namespace PVShra.Functions
{
    public enum FunctionType
    {
        Sin,
        Cos,
        Tan,
        Linear,
        Quadratic,
        Exponential
    }
    
    public interface IFunction
    {
        float Evaluate(float x);
        string GetName();
    }
    
    public class SinFunction : IFunction
    {
        public float Evaluate(float x) => (float)Math.Sin(x);
        public string GetName() => "sin(x)";
    }
    
    public class CosFunction : IFunction
    {
        public float Evaluate(float x) => (float)Math.Cos(x);
        public string GetName() => "cos(x)";
    }
    
    public class TanFunction : IFunction
    {
        public float Evaluate(float x) => (float)Math.Tan(x);
        public string GetName() => "tan(x)";
    }
    
    public class LinearFunction : IFunction
    {
        public float Evaluate(float x) => x;
        public string GetName() => "x";
    }
    
    public class QuadraticFunction : IFunction
    {
        public float Evaluate(float x) => x * x;
        public string GetName() => "x²";
    }
    
    public class ExponentialFunction : IFunction
    {
        public float Evaluate(float x) => (float)Math.Exp(x / 2.0); // Scaled down
        public string GetName() => "e^(x/2)";
    }
    
    public static class FunctionFactory
    {
        public static IFunction CreateFunction(FunctionType type)
        {
            return type switch
            {
                FunctionType.Sin => new SinFunction(),
                FunctionType.Cos => new CosFunction(),
                FunctionType.Tan => new TanFunction(),
                FunctionType.Linear => new LinearFunction(),
                FunctionType.Quadratic => new QuadraticFunction(),
                FunctionType.Exponential => new ExponentialFunction(),
                _ => new SinFunction()
            };
        }
    }
}
