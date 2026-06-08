using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class ArticleService
    {
        private readonly AppDbContext _db;
        private readonly CustomFieldService _customFields;

        public ArticleService(AppDbContext db, CustomFieldService customFields)
        {
            _db = db;
            _customFields = customFields;
        }

        public async Task<List<ArticleResponse>> Get(string? search = null, string? category = null, bool? active = null)
        {
            var query = _db.Articles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();
                query = query.Where(article => article.Code.Contains(normalizedSearch) || article.Name.Contains(normalizedSearch));
            }

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(article => article.Category == category);

            if (active.HasValue)
                query = query.Where(article => article.Active == active.Value);

            return await query.OrderBy(article => article.Code).Select(article => new ArticleResponse
            {
                Id = article.Id,
                Code = article.Code,
                Name = article.Name,
                Category = article.Category,
                Unit = article.Unit,
                Price = article.Price,
                VatRate = article.VatRate,
                Active = article.Active
            }).ToListAsync();
        }

        public async Task<ArticleResponse?> GetById(int id)
        {
            var article = await _db.Articles.FindAsync(id);
            if (article == null)
                return null;

            var response = ToResponse(article);
            response.CustomFields = await _customFields.GetValues("articles", article.Id);
            return response;
        }

        public async Task<ArticleResponse> Save(ArticleRequest request)
        {
            await Validate(request);
            var article = new Article
            {
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                Category = Normalize(request.Category, "Servizi"),
                Unit = Normalize(request.Unit, "pz"),
                Price = request.Price,
                VatRate = request.VatRate,
                Active = request.Active,
                CreatedAt = DateTime.UtcNow
            };
            _db.Articles.Add(article);
            await _db.SaveChangesAsync();
            await _customFields.SaveValues("articles", article.Id, request.CustomFields);
            return await GetById(article.Id) ?? ToResponse(article);
        }

        public async Task<ArticleResponse?> Update(ArticleRequest request)
        {
            var article = await _db.Articles.FindAsync(request.Id);
            if (article == null)
                return null;

            await Validate(request);
            article.Code = request.Code.Trim();
            article.Name = request.Name.Trim();
            article.Category = Normalize(request.Category, "Servizi");
            article.Unit = Normalize(request.Unit, "pz");
            article.Price = request.Price;
            article.VatRate = request.VatRate;
            article.Active = request.Active;
            await _db.SaveChangesAsync();
            await _customFields.SaveValues("articles", article.Id, request.CustomFields);
            return await GetById(article.Id) ?? ToResponse(article);
        }

        public async Task<ArticleResponse?> Toggle(int id)
        {
            var article = await _db.Articles.FindAsync(id);
            if (article == null)
                return null;

            article.Active = !article.Active;
            await _db.SaveChangesAsync();
            return await GetById(article.Id) ?? ToResponse(article);
        }

        public async Task<string> Delete(int id)
        {
            var article = await _db.Articles.FindAsync(id);
            if (article == null)
                return "Articolo non trovato";

            await _customFields.DeleteValues("articles", id);
            _db.Articles.Remove(article);
            await _db.SaveChangesAsync();
            return "Articolo eliminato con successo";
        }

        private async System.Threading.Tasks.Task Validate(ArticleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new Exception("Il codice articolo e obbligatorio");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new Exception("Il nome articolo e obbligatorio");

            var normalizedCode = request.Code.Trim();
            var codeExists = await _db.Articles.AnyAsync(article => article.Code == normalizedCode && article.Id != request.Id);
            if (codeExists)
                throw new Exception("Esiste gia un articolo con questo codice");
        }

        private static string Normalize(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static ArticleResponse ToResponse(Article article)
        {
            return new ArticleResponse
            {
                Id = article.Id,
                Code = article.Code,
                Name = article.Name,
                Category = article.Category,
                Unit = article.Unit,
                Price = article.Price,
                VatRate = article.VatRate,
                Active = article.Active
            };
        }
    }
}








