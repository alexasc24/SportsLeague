using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _service;
    private readonly IMapper _mapper;

    public MatchLineupController(
        IMatchLineupService service,
        IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }


    // Registrar jugador en la alineación

    [HttpPost]
    public async Task<ActionResult<MatchLineupDTO>> AddPlayer(
        int matchId,
        [FromBody] CreateMatchLineupDTO dto)
    {
        try
        {
            var entity = _mapper.Map<MatchLineup>(dto);

            var created = await _service.AddPlayerAsync(matchId, entity);

            var response = _mapper.Map<MatchLineupDTO>(created);

            return CreatedAtAction(
                nameof(GetByMatch),
                new { matchId },
                response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error al registrar el jugador en la alineación del partido.",
                detail = ex.Message
            });
        }
    }

    // Obtener alineación completa del partido

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchLineupDTO>>> GetByMatch(
        int matchId)
    {
        try
        {
            var lineups = await _service.GetByMatchAsync(matchId);

            var response = _mapper.Map<IEnumerable<MatchLineupDTO>>(lineups);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error al consultar la alineación del partido.",
                detail = ex.Message
            });
        }
    }

    // Obtener alineación por equipo

    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupDTO>>> GetByTeam(
        int matchId,
        int teamId)
    {
        try
        {
            var lineups = await _service
                .GetByMatchAndTeamAsync(matchId, teamId);

            var response = _mapper
                .Map<IEnumerable<MatchLineupDTO>>(lineups);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error al consultar la alineación del equipo para este partido",
                detail = ex.Message
            });
        }
    }


    // Eliminar jugador de la alineación

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int matchId,
        int id)
    {
        try
        {
            await _service.DeleteAsync(matchId, id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error al eliminar el jugador de la alineación.",
                detail = ex.Message
            });
        }
    }
}