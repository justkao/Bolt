using System.Threading.Tasks;

namespace TestService.Contracts.Repository
{
    public interface IRepositoryManagement
    {
        void CreateRepository(string server, string computer, int capacity);

        Task CreateRepositoryAsync(string server, string computer, int capacity);

        Task<double> GetRepositoryStatus(string server, string computer, int capacity);
    }
}