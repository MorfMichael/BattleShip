using BattleShip.Data;
using BattleShip.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NBattleshipCodingContest.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleShip.Controllers
{
    public record ShotRequest(BoardIndex? LastShot, BoardContent Board);
    public record FinishedDto(Guid? GameId, BoardContent Board, int NumberOfShots);

    [Route("api/[controller]")]
    [ApiController]
    public class MadMikeController : ControllerBase
    {
        private string _path = "D:\\Temp\\log.txt";

        [HttpGet("getReady")]
        public ActionResult GetReady()
        {
            if (System.IO.File.Exists(_path))
            {
                System.IO.File.Delete(_path);
            }

            return Ok();
        }

        [HttpPost("getShots")]
        public ActionResult<BoardIndex[]> GetShots([FromBody] ShotRequest[] shotRequests)
        {
            var shots = new BoardIndex[shotRequests.Length];
            var range = Enumerable.Range(0, 100).ToArray();

            Parallel.For(0, shotRequests.Length, i =>
            {
                Dictionary<BoardIndex, int> ranking = Enumerable.Range(0, 100).ToDictionary(x => new BoardIndex(x), x => 0);
                var shotRequest = shotRequests[i];
                var board = shotRequest.Board.Select((x, j) => (Content: x, Index: new BoardIndex(j))).ToList();
                var sunkInfo = board.Where(t => t.Content == SquareContent.SunkenShip).Select(t => t.Index).ToList();
                var sunken = Group(sunkInfo);
                var ignorables = sunken.SelectMany(t => t.GetIgnorables()).ToList();

                var hits = board.Where(t => t.Content == SquareContent.HitShip).Select(t => t.Index).ToList();
                var hitShips = Group(hits);


                if (!sunken.Any(x => x.Count == 2))
                    RankShips(shotRequest.Board, range, new int[] { 0, 1 }, ranking, ignorables, hits: hits);

                if (sunken.Count(x => x.Count == 3) < 2)
                    RankShips(shotRequest.Board, range, new int[] { 0, 1, 2 }, ranking, ignorables, sunken.Count(x => x.Count == 3) == 0 ? 2 : 1, hits);

                if (!sunken.Any(x => x.Count == 4))
                    RankShips(shotRequest.Board, range, new int[] { 0, 1, 2, 3 }, ranking, ignorables, hits: hits);

                if (!sunken.Any(x => x.Count == 5))
                    RankShips(shotRequest.Board, range, new int[] { 0, 1, 2, 3, 4 }, ranking, ignorables, hits: hits);

                if (hits.Any())
                {
                    RankHits(shotRequest.Board, hitShips, ranking, ignorables);
                }

                var shot = ranking.OrderByDescending(x => x.Value).FirstOrDefault().Key;

                //System.IO.File.AppendAllText(_path, GetString(shotRequest.Board.ToShortString(), ranking));
                //System.IO.File.AppendAllText(_path, Environment.NewLine + shot + Environment.NewLine);

                shots[i] = shot;
            });

            return shots;
        }

        [HttpPost("finished")]
        public ActionResult Finished([FromBody] FinishedDto[] finished)
        {
            //finished.OrderBy(t => t.NumberOfShots).ToList().ForEach(x => Console.WriteLine(x.NumberOfShots));

            return Ok();
        }

        private List<Ship> Group(IEnumerable<BoardIndex> indices)
        {
            List<Ship> ships = new List<Ship>();

            foreach (var index in indices)
            {
                if (ships.Any(t => t.Contains(index)))
                    continue;

                var ship = ships.FirstOrDefault(x => x.IsNeighbour(index));
                if (ship != null)
                {
                    ship.Add(index);
                }
                else
                {
                    ships.Add(new Ship { index });
                }
            }

            return ships;
        }

        private void RankShips(BoardContent board, int[] range, int[] ship, Dictionary<BoardIndex, int> ranking, List<BoardIndex> ignorables, int value = 1, List<BoardIndex> hits = null)
        {
            var hqry = range.Where(t => t % 10 < 10 - (ship.Length - 1)).Select(t => ship.Select(x => new BoardIndex(t + x)));
            hqry.Where(t => (hits.Any() ? t.Any(x => hits.Contains(x)) : true) && t.All(x => (board[x] == SquareContent.Unknown || board[x] == SquareContent.HitShip) && !ignorables.Contains(x))).SelectMany(x => x).ToList().ForEach(x => ranking[x] += value);
            
            var vqry = range.Where(t => t / 10 < 10 - (ship.Length - 1)).Select(t => ship.Select(x => new BoardIndex(t + (x * 10))));
            vqry.Where(t => (hits.Any() ? t.Any(x => hits.Contains(x)) : true) && t.All(x => (board[x] == SquareContent.Unknown || board[x] == SquareContent.HitShip) && !ignorables.Contains(x))).SelectMany(x => x).ToList().ForEach(x => ranking[x] += value);

        }

        private void RankHits(BoardContent board, List<Ship> hits, Dictionary<BoardIndex, int> ranking, List<BoardIndex> ignorables)
        {
            var relevant = hits.SelectMany(t => t.GetRelevant()).ToList();
            relevant.Where(t => board[t] == SquareContent.Unknown && !ignorables.Contains(t) && ranking[t] != 0).ToList().ForEach(x => ranking[x] += 100);
        }

        private string GetString(string input, Dictionary<BoardIndex, int> ranking)
        {
            // Note string.Create. Read more at
            // https://docs.microsoft.com/en-us/dotnet/api/system.string.create

            static string BuildSeparator(string chars) =>
                string.Create(1 + 10 * 3 + 9 + 1 + 1, chars, (buf, sepChars) =>
                {
                    var i = 0;
                    buf[i++] = sepChars[0];
                    for (var j = 0; j < 10 - 1; j++)
                    {
                        buf[i++] = sepChars[1];
                        buf[i++] = sepChars[1];
                        buf[i++] = sepChars[1];
                        buf[i++] = sepChars[2];
                    }

                    buf[i++] = sepChars[1];
                    buf[i++] = sepChars[1];
                    buf[i++] = sepChars[1];
                    buf[i++] = sepChars[3];

                    buf[i++] = '\n';
                });

            var top = BuildSeparator("┏━┯┓");
            var middle = BuildSeparator("┠─┼┨");
            var bottom = BuildSeparator("┗━┷┛");

            return string.Create((1 + 10 + 9 + 1) * top.Length, (top, middle, bottom), (buf, seps) =>
            {
                var origBuf = buf;
                ((ReadOnlySpan<char>)seps.top).CopyTo(buf);
                buf = buf[seps.top.Length..];
                for (var row = 0; row < 10; row++)
                {
                    buf[0] = '┃';
                    buf = buf[1..];
                    for (var col = 0; col < 10; col++)
                    {
                        int index = col + row * 10;
                        switch (input[col + row * 10])
                        {
                            case 'H':
                                buf[0] = 'x';
                                buf[1] = 'x';
                                buf[2] = 'x';
                                break;
                            case 'X':
                                buf[0] = 'X';
                                buf[1] = 'X';
                                buf[2] = 'X';
                                break;
                        //case SquareContent.Ship:
                        //    buf[0] = '█';
                        //    buf[1] = '█';
                        //    break;
                        case ' ':
                                var rank = ranking[new BoardIndex(index)];
                                string rankString = rank.ToString("000");
                                buf[0] = rankString[0];
                                buf[1] = rankString[1];
                                buf[2] = rankString[2];
                                break;
                            case 'W':
                                buf[0] = '~';
                                buf[1] = '~';
                                buf[2] = '~';
                                break;
                            default:
                                buf[0] = '?';
                                buf[1] = '?';
                                buf[2] = '?';
                                break;
                        }

                        buf = buf[3..];

                        if (col < 9)
                        {
                            buf[0] = '│';
                            buf = buf[1..];
                        }
                    }
                    buf[0] = '┃';
                    buf[1] = '\n';
                    buf = buf[2..];

                    if (row < 9)
                    {
                        ((ReadOnlySpan<char>)seps.middle).CopyTo(buf);
                        buf = buf[seps.middle.Length..];
                    }
                }

                ((ReadOnlySpan<char>)seps.bottom).CopyTo(buf);
            });
        }
    }
}
