namespace SampleProject.Application.Dto
{
    public class OperationResultDto<T>
    {
        public bool Success { get; }
        public T? Data { get; }
        public IEnumerable<Error> Errors { get; }

        internal OperationResultDto(T data) => (Success, Data, Errors) = (true, data, new List<Error>());
        internal OperationResultDto(IEnumerable<Error> errors) => (Success, Errors) = (false, errors);

    }
}
