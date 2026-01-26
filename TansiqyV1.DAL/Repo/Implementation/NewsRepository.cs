using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Repo.Abstraction;

namespace TansiqyV1.DAL.Repo.Implementation;

public class NewsRepository : GenericRepository<News>, INewsRepository
{
    public NewsRepository(ApplicationDbContext context) : base(context)
    {
    }
}

