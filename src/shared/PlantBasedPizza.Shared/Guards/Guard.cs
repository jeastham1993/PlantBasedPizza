using System;

namespace PlantBasedPizza.Shared.Guards
{
    public static class Guard
    {
        public static void AgainstNullOrEmpty(string input, string paramName)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(paramName);
            }
        }
        
        public static void AgainstNull(object input, string paramName)
        {
            if (input == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}