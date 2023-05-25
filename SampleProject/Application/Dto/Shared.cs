namespace SampleProject.Application.Dto
{
    public record EmptyDtoError(string message) : Error(message);

    public static class Shared
    {
        public static Entity<T> VerifyDto<T>(object dto, Func<Entity<T>> notNullDtoFun)
        {
            if (dto is null) return Entity<T>.Invalid(new EmptyDtoError("No data was provided to the operation."));
            else
                return notNullDtoFun();
        }
    }
}
