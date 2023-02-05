namespace Grenat.Functional.DDD
{
    public record Error
    {
        public string Message { get; }
        public string TypeName { get; }
        private string CallStack { get; }

        public Error(string message)
        {
            Message = message;
            TypeName = this.GetType().Name;
            CallStack = Environment.StackTrace;
        }
    }
}
