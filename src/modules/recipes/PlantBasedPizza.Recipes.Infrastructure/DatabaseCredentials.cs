using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantBasedPizza.Recipes.Infrastructure
{
    public class DatabaseCredentials
    {
        public string dbClusterIdentifier { get; set; }
        public string password { get; set; }
        public string engine { get; set; }
        public int port { get; set; }
        public string host { get; set; }
        public bool ssl { get; set; }
        public string username { get; set; }

        public override string ToString()
        {
            return $"mongodb://{username}:{password}@{host}:{port}/?ssl={ssl}&sslVerifyCertificate=false&replicaSet=rs0&readpreference=secondaryPreferred&retryWrites=false";
        }
    }
}
