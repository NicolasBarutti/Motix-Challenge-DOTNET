using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motix.Application.DTOs;
using Motix.Domain.Entities;
using Motix.Extensions;
using Motix.Infrastructure.Persistence;
using Motix.Models;
using Motix.Services;

namespace Motix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectorsController : ControllerBase
{
    private readonly MotixDbContext _ctx;
    public SectorsController(MotixDbContext ctx) => _ctx = ctx;

    /// <summary>Lista setores com paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var paging = new PagingParameters(page, pageSize);
        var paged = await _ctx.Sectors.AsNoTracking()
            .OrderBy(s => s.Code)
            .ToPagedAsync(paging, ct);

        var items = paged.Items.Cast<Sector>().Select(s => new
        {
            data = new SectorDto(s.Id, s.Code),
            _links = LinkFactory.SectorLinks(HttpContext, s.Id)
        });

        return Ok(new PagedResult<object>(items, paged.Page, paged.PageSize, paged.TotalCount));
    }

    /// <summary>Obtém um setor por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SectorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var s = await _ctx.Sectors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();
        return Ok(new { data = new SectorDto(s.Id, s.Code), _links = LinkFactory.SectorLinks(HttpContext, s.Id) });
    }

    /// <summary>Cria um setor.</summary>
    /// <remarks>Exemplo:
    /// 
    ///     POST /api/sectors
    ///     { "code": "A1" }
    /// 
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(SectorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateSectorDto input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
            return BadRequest(new { error = "Code é obrigatório" });

        var s = new Sector { Id = Guid.NewGuid(), Code = input.Code.Trim() };
        _ctx.Sectors.Add(s);
        await _ctx.SaveChangesAsync(ct);

        var dto = new SectorDto(s.Id, s.Code);
        return CreatedAtAction(nameof(GetById), new { id = s.Id }, new { data = dto, _links = LinkFactory.SectorLinks(HttpContext, s.Id) });
    }

    /// <summary>Atualiza um setor.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateSectorDto input, CancellationToken ct = default)
    {
        var s = await _ctx.Sectors.FindAsync(new object?[] { id }, ct);
        if (s is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(input.Code))
            s.Code = input.Code.Trim();

        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Remove um setor.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var s = await _ctx.Sectors.FindAsync(new object?[] { id }, ct);
        if (s is null) return NotFound();

        _ctx.Sectors.Remove(s);
        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }
}
