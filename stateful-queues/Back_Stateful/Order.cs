using System;
using System.Runtime.Serialization;

namespace Back_Stateful
{
    [DataContract(Name = "Order", Namespace = "Back_Stateful")]
    public class Order
    {
        public const string OrdersDictionaryName = "orders";

        [DataMember(IsRequired = true, Name = "OrderId")]
        public Guid OrderId { get; set; }

        [DataMember(IsRequired = true, Name = "SubmittedOn")]
        public DateTime SubmittedOn { get; set; }

        [DataMember(IsRequired = true, Name = "ProcessedOn")]
        public DateTime ProcessedOn { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "Accepted")]
        public bool Accepted { get; set; }
    }
}
