using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetizacionEcosistemaCreador.Common;
using MonetizacionEcosistemaCreador.Data;
using MonetizacionEcosistemaCreador.Models;
using MonetizacionEcosistemaCreador.Models.Dtos;

namespace MonetizacionEcosistemaCreador.Controllers;

/// <summary>Registro y consulta de documentación fiscal para cobros (RF-M5).</summary>
[ApiController]
[Route("api/fiscal-documents")]
public class FiscalDocumentsController : ControllerBase
{
    private readonly MonetizacionEcosistemaCreadorDbContext _db;

    public FiscalDocumentsController(MonetizacionEcosistemaCreadorDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-M5: Registra documentación fiscal requerida para poder cobrar.</summary>
    [HttpPost]
    public async Task<ActionResult<FiscalDocumentResponse>> Create([FromBody] CreateFiscalDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "DocumentNumber es requerido."));
        }

        if (!Enum.TryParse<FiscalDocumentType>(request.DocumentType, ignoreCase: true, out var docType))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                "DocumentType no es válido.", new { allowed = Enum.GetNames<FiscalDocumentType>() }));
        }

        var entity = new FiscalDocument
        {
            Id = Guid.NewGuid(),
            ChannelId = request.ChannelId,
            DocumentType = docType,
            DocumentNumber = request.DocumentNumber,
            Country = request.Country ?? string.Empty,
            FileUrl = request.FileUrl,
            Status = FiscalDocumentStatus.PendingReview,
            SubmittedAt = DateTime.UtcNow
        };

        _db.FiscalDocuments.Add(entity);
        await _db.SaveChangesAsync();

        return Created($"/api/fiscal-documents/{entity.Id}", ToResponse(entity));
    }

    /// <summary>RF-M5: Lista paginada de documentos fiscales de un canal.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<FiscalDocumentResponse>>> List(
        [FromQuery] Guid channelId,
        [FromQuery] PaginationQuery query)
    {
        if (channelId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "channelId es requerido."));
        }

        var baseQuery = _db.FiscalDocuments.AsNoTracking().Where(d => d.ChannelId == channelId);
        var total = await baseQuery.CountAsync();
        var entities = await baseQuery
            .OrderByDescending(d => d.SubmittedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var items = entities.Select(ToResponse).ToList();

        return Ok(PagedResult<FiscalDocumentResponse>.Create(items, query.Page, query.PageSize, total));
    }

    private static FiscalDocumentResponse ToResponse(FiscalDocument d) => new()
    {
        Id = d.Id,
        ChannelId = d.ChannelId,
        DocumentType = d.DocumentType.ToString(),
        DocumentNumber = d.DocumentNumber,
        Country = d.Country,
        Status = d.Status.ToString(),
        SubmittedAt = d.SubmittedAt,
        ReviewedAt = d.ReviewedAt
    };
}
