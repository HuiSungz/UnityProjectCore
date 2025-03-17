
using System.Collections.Generic;

namespace ProjectCore.PlatformAnalysis
{
    internal class EventData
    {
        public string EventName { get; }
        public string ParamValue { get; }
        public Dictionary<string, object> Attributes { get; }
        
        private EventData(Builder builder)
        {
            EventName = builder.EventName;
            ParamValue = builder.ParamValue;
            Attributes = builder.Attributes ?? new Dictionary<string, object>();
        }
        
        public class Builder
        {
            public string EventName { get; private set; }
            public string ParamValue { get; private set; }
            public Dictionary<string, object> Attributes { get; private set; }
            
            public Builder(string eventName)
            {
                EventName = eventName;
                Attributes = new Dictionary<string, object>();
            }
            
            public Builder SetParamValue(string paramValue)
            {
                ParamValue = paramValue;
                return this;
            }
            
            public Builder AddAttribute(string key, object value)
            {
                if (Attributes == null)
                {
                    Attributes = new Dictionary<string, object>();
                }
                
                Attributes[key] = value;
                return this;
            }
            
            public Builder SetAttributes(Dictionary<string, object> attributes)
            {
                Attributes = attributes;
                return this;
            }
            
            public EventData Build()
            {
                return new EventData(this);
            }
        }
    }
}