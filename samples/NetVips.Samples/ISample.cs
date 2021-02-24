namespace NetVips
{
    public interface ISample
    {
        string Name { get; }

        string Category { get; }

        void Execute(string[] args);
    }
}