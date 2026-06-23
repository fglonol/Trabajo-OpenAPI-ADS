using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetizacionEcosistemaCreador.Common;
using MonetizacionEcosistemaCreador.Data;
using MonetizacionEcosistemaCreador.Models;
using MonetizacionEcosistemaCreador.Models.Dtos;

namespace MonetizacionEcosistemaCreador.Controllers;

/// <summary>Solicitud y gestión de pagos/payouts sobre el saldo de un canal (RF-M5).</summary>
[ApiController]
[Route("api/[controller]")]
public class PayoutsController : ControllerBase
{
    private readonly MonetizacionEcosistemaCreadorDbContext _db;

    public PayoutsController(MonetizacionEcosistemaCreadorDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-M5: Solicita un payout. 409 si el monto supera el saldo disponible; en caso contrario crea el pago en estado Pending y descuenta el saldo.</summary>
    [HttpPost]
    public async Task<ActionResult<PayoutResponse>> Create([FromBody] CreatePayoutRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El monto del pago debe ser mayor que cero."));
        }

        if (request.Amount < request.MinimumPayoutAmount)
        {
            return Conflict(new ErrorResponse("CONFLICT",
                $"El monto solicitado ({request.Amount}) no alcanza el mínimo de retiro ({request.MinimumPayoutAmount})."));
        }

        var earnings = await _db.CreatorEarnings.FirstOrDefaultAsync(x => x.ChannelId == request.ChannelId);
        if (earnings is null)
        {
            return Conflict(new ErrorResponse("CONFLICT", "El canal no tiene saldo disponible para pagos."));
        }

        if (request.Amount > earnings.Balance)
        {
            return Conflict(new ErrorResponse("CONFLICT", "El monto solicitado supera el saldo disponible."));
        }

        var now = DateTime.UtcNow;

        var payout = new Payout
        {
            Id = Guid.NewGuid(),
            ChannelId = request.ChannelId,
            Amount = request.Amount,
            MinimumPayoutAmount = request.MinimumPayoutAmount,
            TaxDocumentId = request.TaxDocumentId,
            Status = PayoutStatus.Pending,
            RequestedAt = now
        };
        _db.Payouts.Add(payout);

        earnings.Balance -= request.Amount;
        earnings.UpdatedAt = now;

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(List), new { channelId = payout.ChannelId }, ToResponse(payout));
    }

    /// <summary>RF-M5: Marca un payout como pagado (Paid). 404 si no existe; 409 si no está pendiente.</summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<PayoutResponse>> Complete(Guid id)
    {
        var payout = await _db.Payouts.FirstOrDefaultAsync(p => p.Id == id);
        if (payout is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Pago no encontrado."));
        }

        if (payout.Status != PayoutStatus.Pending)
        {
            return Conflict(new ErrorResponse("CONFLICT", "Solo se pueden completar pagos en estado Pending."));
        }

        payout.Status = PayoutStatus.Paid;
        payout.PaidAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToResponse(payout));
    }

    /// <summary>RF-M5: Marca un payout como fallido (Failed) y reintegra el monto al saldo del canal. 404 si no existe; 409 si no está pendiente.</summary>
    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<PayoutResponse>> Fail(Guid id)
    {
        var payout = await _db.Payouts.FirstOrDefaultAsync(p => p.Id == id);
        if (payout is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Pago no encontrado."));
        }

        if (payout.Status != PayoutStatus.Pending)
        {
            return Conflict(new ErrorResponse("CONFLICT", "Solo se pueden marcar como fallidos los pagos en estado Pending."));
        }

        payout.Status = PayoutStatus.Failed;

        var earnings = await _db.CreatorEarnings.FirstOrDefaultAsync(x => x.ChannelId == payout.ChannelId);
        if (earnings is not null)
        {
            earnings.Balance += payout.Amount;
            earnings.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok(ToResponse(payout));
    }

    /// <summary>RF-M5: Lista paginada de pagos de un canal, opcionalmente filtrados por estado.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<PayoutResponse>>> List(
        [FromQuery] Guid channelId,
        [FromQuery] string? status,
        [FromQuery] PaginationQuery query)
    {
        var baseQuery = _db.Payouts.AsNoTracking().Where(p => p.ChannelId == channelId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<PayoutStatus>(status, ignoreCase: true, out var st))
            {
                return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                    "El estado de pago no es válido.", new { allowed = Enum.GetNames<PayoutStatus>() }));
            }
            baseQuery = baseQuery.Where(p => p.Status == st);
        }

        var total = await baseQuery.CountAsync();
        var entities = await baseQuery
            .OrderByDescending(p => p.RequestedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var items = entities.Select(ToResponse).ToList();

        return Ok(PagedResult<PayoutResponse>.Create(items, query.Page, query.PageSize, total));
    }

    private static PayoutResponse ToResponse(Payout p) => new()
    {
        Id = p.Id,
        ChannelId = p.ChannelId,
        Amount = p.Amount,
        Status = p.Status.ToString(),
        RequestedAt = p.RequestedAt,
        MinimumPayoutAmount = p.MinimumPayoutAmount,
        TaxDocumentId = p.TaxDocumentId,
        PaidAt = p.PaidAt
    };
}
