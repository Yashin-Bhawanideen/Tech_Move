namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for Client responses
    /// Used when returning client data to clients
    /// </summary>
    public class ClientDto
    {
        /// <summary>
        /// Unique identifier for the client
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Name of the client company
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the client
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number of the client
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Physical address of the client
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Geographic region of the client (Africa, Europe, Asia, etc.)
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// Date when the client was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Number of contracts associated with this client
        /// </summary>
        public int ContractsCount { get; set; }
    }
}