using System;

namespace Retorno360Tacna.SERVICES
{
    internal static class SqlHelper
    {
        // Simple QUOTENAME implementation for database/identifier names
        public static string Quotename(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "[]";

            // Remove any surrounding brackets then escape internal closing bracket
            var clean = name.Trim().TrimStart('[').TrimEnd(']');

            // QUOTENAME escapes closing brackets by doubling them
            clean = clean.Replace("]", "]]" );

            return "[" + clean + "]";
        }
    }
}
