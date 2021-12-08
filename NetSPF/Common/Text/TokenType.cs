namespace NetSPF.Common.Text
{
    public enum TokenType
    {
        /// <summary>
        /// No token has been defined.
        /// </summary>
        None = 1,

        /// <summary>
        /// A text.
        /// </summary>
        Text = 2,

        /// <summary>
        /// A number.
        /// </summary>
        Number = 3,

        /// <summary>
        /// A single space character.
        /// </summary>
        Space = 4,

        /// <summary>
        /// A new line token, defined as CRLF.
        /// </summary>
        NewLine = 5,
        
        /// <summary>
        /// A separator token.
        /// </summary>
        Separator = 6,
        
        /// <summary>
        /// A qualifier token.
        /// </summary>
        Qualifier = 7,

        /// <summary>
        /// Unknown.
        /// </summary>
        Other = 8,
    }
}