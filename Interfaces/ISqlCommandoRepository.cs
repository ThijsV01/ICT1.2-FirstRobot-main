public interface ISqlCommandoRepository
{
    Task<int> GetCommands();
    void InsertCommand();
}