using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motix.Application.DTOs;
using Motix.Domain.Entities;
using Motix.Extensions;
using Motix.Infrastructure.Persistence;
using Motix.Models;
using Motix.Services;
using Asp.Versioning;

namespace Motix.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MotorcyclesController : ControllerBase
{
    private readonly MotixDbContext _ctx;
    public MotorcyclesController(MotixDbContext ctx) => _ctx = ctx;

    /// <summary>Lista motos com paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var paging = new PagingParameters(page, pageSize);
        var paged = await _ctx.Motorcycles.AsNoTracking()
            .OrderBy(m => m.Plate)
            .ToPagedAsync(paging, ct);

        var items = paged.Items.Cast<Motorcycle>().Select(m => new
        {
            data = new MotorcycleDto(m.Id, m.Plate, m.SectorId),
            _links = LinkFactory.MotorcycleLinks(HttpContext, m.Id)
        });

        return Ok(new PagedResult<object>(items, paged.Page, paged.PageSize, paged.TotalCount));
    }

    /// <summary>Obtém uma moto por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MotorcycleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var m = await _ctx.Motorcycles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return NotFound();
        return Ok(new { data = new MotorcycleDto(m.Id, m.Plate, m.SectorId), _links = LinkFactory.MotorcycleLinks(HttpContext, m.Id) });
    }

    /// <summary>Cria uma moto.</summary>
    /// <remarks>Exemplo:
    ///
    ///     POST /api/v1/motorcycles
    ///     { "plate": "ABC1D23", "sectorId": "GUID_DO_SETOR" }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(MotorcycleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateMotorcycleDto input, CancellationToken ct = default)
    {
        if (input.SectorId == Guid.Empty)
            return BadRequest(new { error = "SectorId é obrigatório" });

        if (string.IsNullOrWhiteSpace(input.Plate))
            return BadRequest(new { error = "Plate é obrigatória" });

        var m = new Motorcycle
        {
            Id = Guid.NewGuid(),
            Plate = input.Plate.Trim().ToUpperInvariant(),
            SectorId = input.SectorId
        };

        try
        {
            _ctx.Motorcycles.Add(m);
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("ORA-02291") == true)
        {
            // FK inválida (Setor não existe)
            return BadRequest(new { error = "SectorId inexistente (violação de FK)" });
        }

        var dto = new MotorcycleDto(m.Id, m.Plate, m.SectorId);
        // 🔧 inclui a versão na rota de retorno
        return CreatedAtAction(nameof(GetById),
            new { version = "1.0", id = m.Id },
            new { data = dto, _links = LinkFactory.MotorcycleLinks(HttpContext, m.Id) });
    }

    /// <summary>Atualiza uma moto.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateMotorcycleDto input, CancellationToken ct = default)
    {
        var m = await _ctx.Motorcycles.FindAsync(new object?[] { id }, ct);
        if (m is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(input.Plate))
            m.Plate = input.Plate.Trim().ToUpperInvariant();

        if (input.SectorId != Guid.Empty)
            m.SectorId = input.SectorId;

        try
        {
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("ORA-02291") == true)
        {
            return BadRequest(new { error = "SectorId inexistente (violação de FK)" });
        }

        return NoContent();
    }

    /// <summary>Remove uma moto.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var m = await _ctx.Motorcycles.FindAsync(new object?[] { id }, ct);
        if (m is null) return NotFound();

        _ctx.Motorcycles.Remove(m);
        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }
}
