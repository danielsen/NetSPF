namespace NetSPF.Common.Text
{
    public enum ParsingErrorType
    {
        SyntaxError = 1,
        UnknownMechanism = 2,
        ValueError = 3
    }
    
    public class ParsingError
    {
        public ParsingError(ParsingErrorType errorType)
        {
            ErrorType = errorType;
        }

        public ParsingError(ParsingErrorType errorType, string message, string source = null) : this(errorType)
        {
            Message = message;
            Source = source;
        }

        public ParsingErrorType ErrorType { get; }
        public string Message { get; }
        public string Source { get; }

        public static ParsingError CreateForInvalidQualifier(string source = null)
        {
            return new ParsingError(ParsingErrorType.SyntaxError, "Mechanism uses an invalid qualifier.", source);
        }

        public static ParsingError CreateForMissingValue(string source = null)
        {
            return new ParsingError(ParsingErrorType.SyntaxError, "Mechanism is missing a value.", source);
        }

        public static ParsingError CreateForUnknownMechanism(string source = null)
        {
            return new ParsingError(ParsingErrorType.UnknownMechanism, "Unknown mechanism.", source);
        }
        
        public static ParsingError CreateForValueError(string message = null, string source = null)
        {
            return new ParsingError(ParsingErrorType.ValueError, message ?? "Mechanism has in invalid value.", source);
        }
    }
}