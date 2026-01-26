using TansiqyV1.BLL.ModelVM;
using TansiqyV1.BLL.Services.Abstraction;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Repo.Abstraction;

namespace TansiqyV1.BLL.Services.Implementation;

public class NewsService : INewsService
{
    private readonly IGenericRepository<News> _newsRepository;

    public NewsService(IGenericRepository<News> newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public async Task<IEnumerable<NewsViewModel>> GetAllNewsAsync()
    {
        var news = await _newsRepository.GetAllAsync();
        return news.OrderByDescending(n => n.Date).Select(n => new NewsViewModel
        {
            Id = n.Id,
            Title = n.Title,
            Date = n.Date,
            Description = n.Description
        });
    }

    public async Task<NewsViewModel?> GetNewsByIdAsync(int id)
    {
        var news = await _newsRepository.GetByIdAsync(id);
        if (news == null) return null;

        return new NewsViewModel
        {
            Id = news.Id,
            Title = news.Title,
            Date = news.Date,
            Description = news.Description
        };
    }

    public async Task<NewsViewModel> CreateNewsAsync(CreateNewsDto dto)
    {
        var news = new News
        {
            Title = dto.Title,
            Date = dto.Date,
            Description = dto.Description,
            CreatedAt = DateTime.Now
        };

        var createdNews = await _newsRepository.AddAsync(news);

        return new NewsViewModel
        {
            Id = createdNews.Id,
            Title = createdNews.Title,
            Date = createdNews.Date,
            Description = createdNews.Description
        };
    }

    public async Task<NewsViewModel> UpdateNewsAsync(UpdateNewsDto dto)
    {
        var news = await _newsRepository.GetByIdAsync(dto.Id);
        if (news == null)
            throw new ArgumentException($"News with ID {dto.Id} not found");

        news.Title = dto.Title;
        news.Date = dto.Date;
        news.Description = dto.Description;
        news.UpdatedAt = DateTime.Now;

        await _newsRepository.UpdateAsync(news);

        return new NewsViewModel
        {
            Id = news.Id,
            Title = news.Title,
            Date = news.Date,
            Description = news.Description
        };
    }

    public async Task<bool> DeleteNewsAsync(int id)
    {
        var news = await _newsRepository.GetByIdAsync(id);
        if (news == null)
            return false;

        await _newsRepository.DeleteAsync(id);
        return true;
    }
}

