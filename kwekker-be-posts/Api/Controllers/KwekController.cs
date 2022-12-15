#nullable disable
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[Route("kweks")]
[ApiController]
public class KwekController : ControllerBase
{
    private readonly KwekkerContext _context;

    public KwekController(KwekkerContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<KwekOutputDTO>>> GetKweks([FromQuery(Name = "username")] string? username)
    {
      if (!string.IsNullOrWhiteSpace(username))
      {
        return await _context.Kweks
         .Where(k => k.User.Username == username)
         .Include(k => k.User)
         .OrderByDescending(k => k.PostedAt)
         .Select(k => KwekToOutputDto(k))
         .ToListAsync();
      }

      return await _context.Kweks
         .Include(k => k.User)
         .OrderByDescending(k => k.PostedAt)
         .Select(k => KwekToOutputDto(k))
         .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<KwekOutputDTO>> GetKwek(int id)
    {
        var kwek = await _context.Kweks
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kwek == null)
        {
            return NotFound();
        }
            
        var kwekOutputDto = KwekToOutputDto(kwek);

        return kwekOutputDto;
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutKwek(int id, KwekInputDTO kwekInputDto)
    {
        var kwek = await _context.Kweks
            .Include(k => k.User)
            .Where(k => k.Id == id)
            .SingleOrDefaultAsync();
            
        if (kwek == null)
        {
            return NotFound();
        }
        
        var userProviderId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var user = await _context.Users.FirstAsync(u => u.ProviderId == userProviderId);

        if (kwek.User.Id != user.Id)
        {
            return Forbid();
        }

        kwek.Text = kwekInputDto.Text;
            
        _context.Entry(kwek).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Kwek>> PostKwek(KwekInputDTO kwekDto)
    {
        var userProviderId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var user = await _context.Users.FirstAsync(u => u.ProviderId == userProviderId);
        
        var kwek = new Kwek
        {
            Text = kwekDto.Text,
            PostedAt = DateTime.UtcNow,
            User = user
        };

        _context.Kweks.Add(kwek);
        await _context.SaveChangesAsync();

        var kwekOutputDto = KwekToOutputDto(kwek);

        return CreatedAtAction(nameof(GetKwek), new { id = kwek.Id }, kwekOutputDto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteKwek(int id)
    {
        var kwek = await _context.Kweks
            .Include(k => k.User)
            .Where(k => k.Id == id)
            .SingleOrDefaultAsync();
        
        if (kwek == null)
        {
            return NotFound();
        }

        var userProviderId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var user = await _context.Users.FirstAsync(u => u.ProviderId == userProviderId);

        if (kwek.User.Id != user.Id)
        {
            return Forbid();
        }

        _context.Kweks.Remove(kwek);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static KwekOutputDTO KwekToOutputDto(Kwek kwek)
    {
        return new KwekOutputDTO
        {
            Id = kwek.Id,
            Text = kwek.Text,
            User = new UserOutputDTO
            {
                Username = kwek.User.Username,
                DisplayName = kwek.User.DisplayName,
                AvatarUrl = kwek.User.AvatarUrl
            },
            PostedAt = kwek.PostedAt
        };
    }
}