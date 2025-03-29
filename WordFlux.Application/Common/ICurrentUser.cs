namespace WordFlux.Application;

public interface ICurrentUser
{
    Guid? GetUserId();
}