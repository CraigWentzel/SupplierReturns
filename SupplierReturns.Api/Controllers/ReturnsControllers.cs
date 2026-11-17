using Microsoft.AspNetCore.Mvc;
using SupplierReturns.Api.Data;
using SupplierReturns.Api.Dtos;
using SupplierReturns.Api.Models;

namespace SupplierReturns.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReturnsController : ControllerBase
{
    private readonly ReturnData _data;

    public ReturnsController(ReturnData data)
    {
        _data = data;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReturnDto>>> GetAll(
        [FromQuery] ReturnStatus? status,
        [FromQuery] int? supplierId)
    {
        List<ReturnDto> returns = await _data.GetAllAsync(status, supplierId);
        return Ok(returns);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReturnDto>> GetById(int id)
    {
        ReturnDto? dto = await _data.GetByIdAsync(id);
        if (dto == null)
        {
            return NotFound();
        }
        return Ok(dto);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReturnDto>> Create(CreateReturnDto input)
    {
        ReturnDto? dto = await _data.CreateAsync(input);
        if (dto == null)
        {
            return BadRequest(new { error = "Supplier not found." });
        }
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReturnDto>> Submit(int id)
    {
        (ReturnDto? Dto, string? Error) result = await _data.SubmitAsync(id);
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/credit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReturnDto>> Credit(int id, CreditReturnDto input)
    {
        (ReturnDto? Dto, string? Error) result = await _data.CreditAsync(id, input.CreditNoteNumber);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        string? error = await _data.DeleteAsync(id);
        if (error == ReturnData.NotFound)
        {
            return NotFound();
        }
        if (error != null)
        {
            return Conflict(new { error });
        }
        return NoContent();
    }

    private ActionResult<ReturnDto> ToActionResult((ReturnDto? Dto, string? Error) result)
    {
        if (result.Error == ReturnData.NotFound)
        {
            return NotFound();
        }
        if (result.Error != null)
        {
            return Conflict(new { error = result.Error });
        }
        return Ok(result.Dto);
    }
}