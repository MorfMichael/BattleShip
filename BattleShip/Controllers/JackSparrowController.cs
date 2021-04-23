using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NBattleshipCodingContest.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleShip.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class JackSparrowController : ControllerBase
    {
        public record ShotRequest(BoardIndex? LastShot, BoardContent Board);
        public record FinishedDto(Guid? GameId, BoardContent Board, int NumberOfShots);
        /// <summary>
        /// Get player ready
        /// </summary>
        /// <returns>OK</returns>
        [HttpGet("getReady")]
        public ActionResult GetReady() => Ok();

        /// <summary>
        /// Calculates the next shots for the given boards.
        /// </summary>
        /// <param name="shotRequests">Shot requests</param>
        /// <returns>Shots</returns>
        /// <remarks>
        /// Your player has to play multiple simultaneous games. So you player receives
        /// multiple shot requests for multiple games running in parallel. Therefore, your
        /// player must calculate a shot for each shot request in <paramref name="shotRequests"/>.
        /// </remarks>
        [HttpPost("getShots")]
        public ActionResult<BoardIndex[]> GetShots([FromBody] ShotRequest[] shotRequests)
        {
            // Create a helper variable that will receive our calculated
            // shots for each shot request.
            var shots = new BoardIndex[shotRequests.Length];

            // Loop over all shot requests
            for (var i = 0; i < shotRequests.Length; i++)
            {
                // Get the current shot request
                var shotRequest = shotRequests[i];

                // If there has not been a previous shot, shoot on A1.
                // Otherwise, shoot on the next square. To calculate the next square,
                // we can use a helper function of `BoardIndex`.
                if (shotRequest.LastShot == null) shots[i] = "A1";
                else shots[i] = shotRequest.LastShot.Value.Next();
            }

            return shots;
        }

        [HttpPost("finished")]
        public ActionResult Finished([FromBody] FinishedDto[] finished) => Ok();
    }
}
