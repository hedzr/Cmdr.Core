using System;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Tool
{
    [AttributeUsage(AttributeTargets.All)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class DeveloperAttribute : Attribute
    {
        private string level;

        // Private fields.
        private string name;
        private bool reviewed;

        // This constructor defines two required parameters: name and level.

        public DeveloperAttribute(string name, string level)
        {
            this.name = name;
            this.level = level;
            reviewed = false;
        }

        // Define Name property.
        // This is a read-only attribute.

        public virtual string Name => name;

        // Define Level property.
        // This is a read-only attribute.

        public virtual string Level => level;

        // Define Reviewed property.
        // This is a read/write attribute.

        public virtual bool Reviewed
        {
            get => reviewed;
            set => reviewed = value;
        }
    }

    [Developer("Joan Smith", "42", Reviewed = true)]
    public class CustomAttributeDemo
    {
        public static void Main()
        {
            // Call function to get and display the attribute.
            GetAttribute(typeof(CustomAttributeDemo));
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void GetAttribute(Type t)
        {
            // Get instance of the attribute.
            var myAttribute = (DeveloperAttribute) Attribute.GetCustomAttribute(t, typeof(DeveloperAttribute));

            if (myAttribute == null)
            {
                Console.WriteLine("The attribute was not found.");
            }
            else
            {
                // Get the Name value.
                Console.WriteLine("The Name Attribute is: {0}.", myAttribute.Name);
                // Get the Level value.
                Console.WriteLine("The Level Attribute is: {0}.", myAttribute.Level);
                // Get the Reviewed value.
                Console.WriteLine("The Reviewed Attribute is: {0}.", myAttribute.Reviewed);
            }
        }
    }
}