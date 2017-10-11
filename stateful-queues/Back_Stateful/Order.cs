using System;
using System.Runtime.Serialization;

namespace Back_Stateful
{
    [DataContract(Name = "Order", Namespace = "Back_Stateful")]
    public class Order
    {
        public const string OrdersDictionaryName = "orders";

        static Random random = new Random();

        public Order()
        {
            OrderId = random.Next();
        }

        [DataMember(IsRequired = true, Name = "OrderId")]
        public int OrderId { get; set; }

        [DataMember(IsRequired = true, Name = "ConfirmationId")]
        public int ConfirmationId { get; set; }

        [DataMember(IsRequired = true, Name = "SubmittedOn")]
        public DateTime SubmittedOn { get; set; }

        [DataMember(IsRequired = true, Name = "ProcessedOn")]
        public DateTime ProcessedOn { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "Accepted")]
        public bool Accepted { get; set; }
    }
}
