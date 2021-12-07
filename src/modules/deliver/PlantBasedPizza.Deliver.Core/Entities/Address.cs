namespace PlantBasedPizza.Deliver.Core.Entities
{
    public class Address
    {
        public Address(string addressLine1, string postcode) : this(addressLine1, string.Empty, string.Empty, string.Empty, string.Empty, postcode)
        {
        }
        
        public Address(string addressLine1, string addressLine2, string addressLine3, string addressLine4, string addressLine5, string postcode)
        {
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            AddressLine3 = addressLine3;
            AddressLine4 = addressLine4;
            AddressLine5 = addressLine5;
            Postcode = postcode;
        }

        public string AddressLine1 { get; private set; }
        
        public string AddressLine2 { get; private set; }
        
        public string AddressLine3 { get; private set; }
        
        public string AddressLine4 { get; private set; }
        
        public string AddressLine5 { get; private set; }
        
        public string Postcode { get; private set; }
        
        
    }
}