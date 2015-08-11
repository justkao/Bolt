namespace Bolt.Server
{
    public interface IBoltFeature
    {
        ServerActionContext ActionContext { get; }
    }
}