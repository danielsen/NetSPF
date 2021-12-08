using System;

namespace NetSPF.Common.Text
{
    public class Token
    {
        public static readonly Token None = new Token(TokenType.None, string.Empty);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The token kind.</param>
        /// <param name="text">The text that the token represents.</param>
        private Token(TokenType type, string text)
        {
            Type = type;
            Text = text;
        }

        /// <summary>
        /// Create a token for the given text.
        /// </summary>
        /// <param name="text">The text string to create the token for.</param>
        /// <returns>The token that was created.</returns>
        public static Token Create(string text)
        {
            return Create(TokenType.Text, text);
        }

        /// <summary>
        /// Create a token for the given text.
        /// </summary>
        /// <param name="type">The token kind.</param>
        /// <param name="text">The text string to create the token for.</param>
        /// <returns>The token that was created.</returns>
        public static Token Create(TokenType type, string text)
        {
            return new Token(type, text);
        }

        /// <summary>
        /// Create a token for the given character.
        /// </summary>
        /// <param name="ch">The character to create the token for.</param>
        /// <returns>The token that was created.</returns>
        public static Token Create(char ch)
        {
            return new Token(KindOf(ch), ch.ToString());
        }

        /// <summary>
        /// Create a token for the given character.
        /// </summary>
        /// <param name="type">The token kind.</param>
        /// <param name="ch">The character to create the token for.</param>
        /// <returns>The token that was created.</returns>
        public static Token Create(TokenType type, char ch)
        {
            return new Token(KindOf(ch), ch.ToString());
        }

        /// <summary>
        /// Create a token for the given byte value.
        /// </summary>
        /// <param name="b">The byte value to create the token for.</param>
        /// <returns>The token that was created.</returns>
        public static Token Create(byte b)
        {
            return Create((char) b);
        }

        /// <summary>
        /// Create a token for the given byte value.
        /// </summary>
        /// <param name="type">The token kind.</param>
        /// <param name="b">The byte value to create the token for.</param>
        /// <returns>The token that was created.</returns>
        public static Token Create(TokenType type, byte b)
        {
            return Create(type, (char) b);
        }

        /// <summary>
        /// Returns the token kind for the given byte value.
        /// </summary>
        /// <param name="value">The byte value to return the token kind for.</param>
        /// <returns>The token kind for the given byte value.</returns>
        public static TokenType KindOf(char value)
        {
            return KindOf((byte) value);
        }

        /// <summary>
        /// Returns the token kind for the given byte value.
        /// </summary>
        /// <param name="value">The byte value to return the token kind for.</param>
        /// <returns>The token kind for the given byte value.</returns>
        public static TokenType KindOf(byte value)
        {
            if (IsText(value))
            {
                return TokenType.Text;
            }

            if (IsNumber(value))
            {
                return TokenType.Number;
            }

            if (IsWhiteSpace(value))
            {
                return TokenType.Space;
            }

            if (IsSeparator(value))
            {
                return TokenType.Separator;
            }

            if (IsQualifier(value))
            {
                return TokenType.Qualifier;
            }

            return TokenType.Other;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a carriage return (CR).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a carriage return (CR) character, false if not.</returns>
        // ReSharper disable once InconsistentNaming
        public static bool IsCR(byte value)
        {
            return value == 13;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a line feed (LF).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a line feed (LF) character, false if not.</returns>
        // ReSharper disable once InconsistentNaming
        public static bool IsLF(byte value)
        {
            return value == 10;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a text or number character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a text or number character, false if not.</returns>
        public static bool IsTextOrNumber(byte value)
        {
            return IsText(value) || IsNumber(value);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a text character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a text character, false if not.</returns>
        public static bool IsText(byte value)
        {
            return IsBetween(value, 65, 90) || IsBetween(value, 97, 122) || IsUtf8(value);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is a UTF-8 encoded character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a UTF-8 character, false if not.</returns>
        private static bool IsUtf8(byte value)
        {
            return value >= 0x80;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a digit character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a digit character, false if not.</returns>
        public static bool IsNumber(byte value)
        {
            return IsBetween(value, 48, 57);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a whitespace.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a whitespace character, false if not.</returns>
        public static bool IsWhiteSpace(byte value)
        {
            return value == 32 || IsBetween(value, 9, 13);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is consider a separator.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is a separator, false if not.</returns>
        private static bool IsSeparator(byte value)
        {
            return value == 47 || value == 58 || value == 61;
        }

        private static bool IsQualifier(byte value)
        {
            return value == 43 || value == 45 || value == 126 || value == 63;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given value is inclusively between a given range.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="low">The lower value of the range.</param>
        /// <param name="high">The higher value of the range.</param>
        /// <returns>true if the value is between the range, false if not.</returns>
        private static bool IsBetween(byte value, byte low, byte high)
        {
            return value >= low && value <= high;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="other">Another object to compare to. </param>
        /// <returns>true if <paramref name="other"/> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public bool Equals(Token other)
        {
            return Type == other.Type && string.Equals(Text, other.Text, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to. </param>
        /// <returns>true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Token && Equals((Token) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Text.GetHashCode() * 397) ^ (int) Type;
            }
        }

        /// <summary>
        /// Returns a value indicating the equality of the two objects.
        /// </summary>
        /// <param name="left">The left hand side of the comparisson.</param>
        /// <param name="right">The right hand side of the comparisson.</param>
        /// <returns>true if the left and right side are equal, false if not.</returns>
        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating the inequality of the two objects.
        /// </summary>
        /// <param name="left">The left hand side of the comparisson.</param>
        /// <param name="right">The right hand side of the comparisson.</param>
        /// <returns>false if the left and right side are equal, true if not.</returns>
        public static bool operator !=(Token left, Token right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the string representation of the token.
        /// </summary>
        /// <returns>The string representation of the token.</returns>
        public override string ToString()
        {
            return $"[{Type}] {Text}";
        }

        /// <summary>
        /// Gets the token kind.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Returns the text representation of the token.
        /// </summary>
        public string Text { get; }
    }
}