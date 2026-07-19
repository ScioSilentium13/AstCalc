namespace AstCalculator.Core.Exceptions
{
    public class AstException : Exception
    {
        public int? Position { get; set; }
        public AstException()
        {
        }

        public AstException(string? message) : base(message)
        {
        }

        /// <summary>
        /// This overload is used to throw an exception with a position there the parsing error occurred.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        public AstException(string? message, int position) : base(message)
        {
            Position = position;
        }

        public AstException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
