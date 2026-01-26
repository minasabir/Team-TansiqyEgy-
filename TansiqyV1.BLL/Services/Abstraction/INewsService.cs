using TansiqyV1.BLL.ModelVM;

namespace TansiqyV1.BLL.Services.Abstraction;

public interface INewsService
{
    Task<IEnumerable<NewsViewModel>> GetAllNewsAsync();
    Task<NewsViewModel?> GetNewsByIdAsync(int id);
    Task<NewsViewModel> CreateNewsAsync(CreateNewsDto dto);
    Task<NewsViewModel> UpdateNewsAsync(UpdateNewsDto dto);
    Task<bool> DeleteNewsAsync(int id);
}

