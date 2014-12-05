namespace Bolt.Generators
{
    public interface IUserCodeGenerator
    {
        void Generate(ClassGenerator generator, object context);
    }
}