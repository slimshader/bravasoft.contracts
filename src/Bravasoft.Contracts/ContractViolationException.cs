using System;

namespace Bravasoft.Contracts
{
    /// <summary>
    /// Thrown when a contract enforced by a type in this library is violated.
    /// </summary>
    public class ContractViolationException : Exception
    {
        /// <summary>Initializes a new instance with the specified message.</summary>
        /// <param name="message">Description of the violated contract.</param>
        public ContractViolationException(string message)
            : base(message)
        {
        }
    }
}
