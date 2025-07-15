namespace Grenat.Functional.DDD.Tests;

public class TestBase
{
    public Action<string> SimpleAction = _ => { };
    public AsyncAction<string> SimpleAsyncAction = async _ => { await Task.FromResult(string.Empty); };

    public Func<int, Entity<int>> IncrementByOne = (int x) => Entity<int>.Valid(x + 1);
    public Func<int, int, int> AddTwoNumbers = (int x, int y) => x + y;
    public Func<int, int, Entity<int>> AddTwoNumbersValid = (int x, int y) => Entity<int>.Valid(x + y);
    public Func<int, int, int, int> AddThreeNumbers = (int x, int y, int z) => x + y + z;
    public Func<int, int, int, int,int> AddFourNumbers = (int w, int x, int y, int z) => w + x + y + z;

    public AsyncFunc<int, int> IncrementByOneAsync = (int x) => Task.FromResult(x + 1);
    public AsyncFunc<int, Entity<int>> IncrementByOneValidAsync = (int x) => Task.FromResult(Entity<int>.Valid(x + 1));
    public AsyncFunc<int, int, int> AddTwoNumbersAsync = (int x, int y) => Task.FromResult(x + y);
    public AsyncFunc<int, int, Entity<int>> AddTwoNumbersValidAsync = (int x, int y) => Task.FromResult(Entity<int>.Valid(x + y));
    public AsyncFunc<int, int, int, Entity<int>> AddThreeNumbersValidAsync = (int x, int y, int z) => Task.FromResult(Entity<int>.Valid(x + y + z));
}
