using Microsoft.AspNetCore.Mvc;
using SupplierReturns.Api.Data;
using SupplierReturns.Api.Dtos;

namespace SupplierReturns.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly SupplierData supplierData;

    public SuppliersController(SupplierData supplierData)
    {
        this.supplierData = supplierData;
    }

    [HttpGet]
    public async Task<ActionResult<List<SupplierDto>>> GetAll()
    {
        List<SupplierDto> suppliers = await supplierData.GetAllAsync();
        return Ok(suppliers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDto>> GetById(int id)
    {
        SupplierDto? supplier = await supplierData.GetByIdAsync(id);
        if (supplier == null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(SupplierSaveDto input)
    {
        SupplierDto created = await supplierData.CreateAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SupplierSaveDto input)
    {
        bool updated = await supplierData.UpdateAsync(id, input);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await supplierData.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
