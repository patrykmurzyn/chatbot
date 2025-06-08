namespace ChatbotAI.Domain.DTOs
{
    /// <summary>
    /// Data transfer object representing a character.
    /// </summary>
    public class CharacterDto
    {
        /// <summary>
        /// Identifier of the character.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Key (name) of the character.
        /// </summary>
        public string Key { get; set; } = null!;
    }
} 