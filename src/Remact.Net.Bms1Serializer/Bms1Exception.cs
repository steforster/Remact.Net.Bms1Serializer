namespace Remact.Net.Bms1Serializer
{
    using System;
    
    public class Bms1Exception : Exception
    {
        public Bms1Exception(string message) : base(message)
        {}
        
        public Bms1Exception(string message, Exception innerEx) : base(message, innerEx)
        {}
    }
}
