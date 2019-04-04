namespace NetVips
{
    public interface ISample
    {
        string Name { get; }

        string Category { get; }

        string Execute(string[] args);
    }
}